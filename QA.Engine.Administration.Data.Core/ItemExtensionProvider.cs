﻿using Dapper;
using Microsoft.Extensions.Logging;
using QA.DotNetCore.Engine.Persistent.Interfaces;
using QA.Engine.Administration.Data.Interfaces.Core;
using QA.Engine.Administration.Data.Interfaces.Core.Models;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace QA.Engine.Administration.Data.Core
{
    public class ItemExtensionProvider: IItemExtensionProvider
    {
        private readonly IUnitOfWork _uow;
        private readonly ILogger<ItemExtensionProvider> _logger;

        public ItemExtensionProvider(IUnitOfWork uow, ILogger<ItemExtensionProvider> logger)
        {
            _uow = uow;
            _logger = logger;
        }

        private const string CmdGetExtantionItems = @"
SELECT 
    ca.ATTRIBUTE_NAME AS FieldName,
	a.TYPE_NAME AS TypeName,
	a.DESCRIPTION AS TypeDescription,
    ca.ATTRIBUTE_ID AS AttributeId
FROM CONTENT_ATTRIBUTE ca JOIN ATTRIBUTE_TYPE a ON ca.ATTRIBUTE_TYPE_ID=a.ATTRIBUTE_TYPE_ID
WHERE ca.CONTENT_ID={0} AND ca.ATTRIBUTE_NAME <> 'ItemId' AND (ca.REQUIRED=1 OR ca.view_in_list=1)

SELECT * FROM content_{0}_united WHERE ItemId={1}

SELECT b.ATTRIBUTE_NAME as 'FieldName', a.CONTENT_ID as 'ExtensionId'
FROM CONTENT_ATTRIBUTE a JOIN (
	SELECT ATTRIBUTE_ID, ATTRIBUTE_NAME, RELATED_ATTRIBUTE_ID, BACK_RELATED_ATTRIBUTE_ID 
	FROM CONTENT_ATTRIBUTE 
	WHERE CONTENT_ID={0} AND (RELATED_ATTRIBUTE_ID is not null OR BACK_RELATED_ATTRIBUTE_ID is not null)) b
	ON a.ATTRIBUTE_ID=b.RELATED_ATTRIBUTE_ID OR a.ATTRIBUTE_ID=b.BACK_RELATED_ATTRIBUTE_ID
";
        private const string CmdGetContentAttribute = @"
SELECT
    ca.ATTRIBUTE_ID AS AttributeId,
    ca.CONTENT_ID AS ContentId,
    ca.ATTRIBUTE_NAME AS AttributeName,
    ca.RELATED_ATTRIBUTE_ID AS RelatedAttributeId,
    LOWER(t.TYPE_NAME) AS AttributeType,
    ca.BACK_RELATED_ATTRIBUTE_ID AS BackRelatedAttributeId,
    ca.TREE_ORDER_FIELD AS TreeOrderField
FROM CONTENT_ATTRIBUTE ca JOIN ATTRIBUTE_TYPE t ON ca.ATTRIBUTE_TYPE_ID=t.ATTRIBUTE_TYPE_ID
WHERE ca.ATTRIBUTE_ID=@attributeId
";
        private const string CmdGetContentFieldValue = @"
SELECT {1} FROM content_{0}_united c WHERE CONTENT_ITEM_ID=@id
";
        private const string CmdGetContentIdByItemId = @"
SELECT CONTENT_ITEM_ID FROM content_{0}_united WHERE ItemId=@itemId
";
        private const string CmdGetContentFieldValues = @"
SELECT CONTENT_ITEM_ID AS ContentId, {1} AS Name FROM content_{0}_united WHERE {2}=@itemId
";

        public List<FieldAttributeData> GetItemExtensionFields(int siteId, int value, int extensionId)
        {
            _logger.LogDebug($"getItemExtensionFields. siteId: {siteId}, value: {value}, extensionId: {extensionId}");
            var query = string.Format(CmdGetExtantionItems, extensionId, value);
            using (var multi = _uow.Connection.QueryMultiple(query))
            {
                var fieldNames = multi.Read<FieldAttributeData>().ToList();
                var contentRow = multi.ReadFirstOrDefault<object>();
                var relatedFields = multi.Read<RelationExtension>().ToList();

                var dict = contentRow as IDictionary<string, object>;

                foreach (var field in fieldNames)
                {
                    field.Value = dict.Where(x => x.Key == field.FieldName).FirstOrDefault().Value;
                    field.RelationExtensionId = relatedFields.FirstOrDefault(x => x.FieldName == field.FieldName)?.ExtensionId;
                }

                _logger.LogDebug($"getItemExtensionFields. fieldNames: {SerializeData(fieldNames)}");

                return fieldNames;
            }
        }

        public string GetRelatedItemName(int siteId, int id, int attributeId)
        {
            _logger.LogDebug($"GetRelatedItemName. siteId: {siteId}, id: {id}, attributeId: {attributeId}");
            var contentAttribute = GetContentAttribute(attributeId);
            while (contentAttribute.RelatedAttributeId.HasValue)
            {
                contentAttribute = GetContentAttribute(contentAttribute.RelatedAttributeId.Value);
                if(contentAttribute.RelatedAttributeId.HasValue)
                {
                    var fieldValue = GetContentFieldValue(id, contentAttribute.ContentId, contentAttribute.AttributeName);
                    if (!int.TryParse(fieldValue, out id))
                    {
                        _logger.LogDebug($"GetRelatedItemName. result: {fieldValue}");
                        return fieldValue;
                    }
                }
            }
            var result = GetContentFieldValue(id, contentAttribute.ContentId, contentAttribute.AttributeName);
            _logger.LogDebug($"GetRelatedItemName. result: {result}");
            return result;
        }

        public Dictionary<int, string> GetManyToOneRelatedItemNames(int siteId, int id, int value, int attributeId)
        {
            _logger.LogDebug($"GetManyToOneRelatedItemNames. siteId: {siteId}, id: {id}, value: {value}, attributeId: {attributeId}");
            var contentAttribute = GetContentAttribute(attributeId);
            if (!contentAttribute.TreeOrderField.HasValue)
            {
                _logger.LogDebug($"GetManyToOneRelatedItemNames. result: null");
                return null;
            }
            var itemId = GetContentIdByItemId(value, contentAttribute.ContentId);
            var nameAttribute = GetContentAttribute(contentAttribute.TreeOrderField.Value);
            var relatedAttribute = GetContentAttribute(id);
            var result = GetContentFieldValues(itemId, nameAttribute.ContentId, nameAttribute.AttributeName, relatedAttribute.AttributeName);
            _logger.LogDebug($"GetManyToOneRelatedItemNames. result: {SerializeData(result)}");
            return result;
        }

        private ContentAttribute GetContentAttribute(int attributeId)
        {
            _logger.LogDebug($"GetContentAttribute. attributeId: {attributeId}");
            var result = _uow.Connection.QuerySingleOrDefault<ContentAttribute>(CmdGetContentAttribute, new { attributeId });
            _logger.LogDebug($"GetContentAttribute. result: {SerializeData(result)}");
            return result;
        }

        private string GetContentFieldValue(int id, int contentId, string attributeName)
        {
            _logger.LogDebug($"GetContentFieldValue. id: {id}, contentId: {contentId}, attributeName: {attributeName}");
            var query = string.Format(CmdGetContentFieldValue, contentId, attributeName);
            var result = _uow.Connection.QuerySingleOrDefault<string>(query, new { id });
            _logger.LogDebug($"GetContentFieldValue. result: {result}");
            return result?.ToString();
        }

        private Dictionary<int, string> GetContentFieldValues(int itemId, int contentId, string titleAttributeName, string attributeName)
        {
            _logger.LogDebug($"GetContentFieldValue. contentId: {contentId}, titleAttributeName: {titleAttributeName}, attributeName: {attributeName}");
            var query = string.Format(CmdGetContentFieldValues, contentId, titleAttributeName, attributeName);
            var result = _uow.Connection.Query<RelatedItem>(query, new { itemId }).ToDictionary(k => k.ContentId, v => v.Name);
            _logger.LogDebug($"GetContentFieldValue. result: {SerializeData(result)}");
            return result;
        }

        private int GetContentIdByItemId(int id, int contentId)
        {
            _logger.LogDebug($"GetContentIdByItemId. id: {id}, contentId: {contentId}");
            var query = string.Format(CmdGetContentIdByItemId, contentId);
            var result = _uow.Connection.QuerySingleOrDefault<int>(query, new { itemId = id });
            _logger.LogDebug($"GetContentIdByItemId. result: {result}");
            return result;
        }

        private static string SerializeData(object data) => Newtonsoft.Json.JsonConvert.SerializeObject(data);

        class RelationExtension
        {
            public string FieldName { get; set; }
            public int ExtensionId { get; set; }
        }

        class ContentAttribute
        {
            public int AttributeId { get; set; }
            public int ContentId { get; set; }
            public string AttributeName { get; set; }
            public int? RelatedAttributeId { get; set; }
            public string AttributeType { get; set; }
            public int? BackRelatedAttributeId { get; set; }
            public int? TreeOrderField { get; set; }
        }

        class RelatedItem
        {
            public int ContentId { get; set; }
            public string Name { get; set; }
        }
    }
}
