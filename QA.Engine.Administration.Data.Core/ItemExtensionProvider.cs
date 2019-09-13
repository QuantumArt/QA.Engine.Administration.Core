using System;
using Dapper;
using Microsoft.Extensions.Logging;
using QA.DotNetCore.Engine.Persistent.Interfaces;
using QA.Engine.Administration.Data.Interfaces.Core;
using QA.Engine.Administration.Data.Interfaces.Core.Models;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using QA.DotNetCore.Engine.Persistent.Dapper;

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

        private const string CmdGetFieldNames = @"
SELECT 
    ca.ATTRIBUTE_NAME AS FieldName,
	a.TYPE_NAME AS TypeName,
	a.DESCRIPTION AS TypeDescription,
    ca.ATTRIBUTE_ID AS AttributeId
FROM CONTENT_ATTRIBUTE ca JOIN ATTRIBUTE_TYPE a ON ca.ATTRIBUTE_TYPE_ID=a.ATTRIBUTE_TYPE_ID
WHERE ca.CONTENT_ID={0} AND ca.ATTRIBUTE_NAME <> 'ItemId' AND (ca.REQUIRED=1 OR ca.view_in_list={1})";
        
        private const string CmdGetContentRow = @"SELECT * FROM content_{0}_united WHERE ItemId={1}";
        private const string CmdGetExtentionItems = @"
SELECT b.ATTRIBUTE_NAME as FieldName, a.CONTENT_ID as ExtensionId
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

        public List<FieldAttributeData> GetItemExtensionFields(int siteId, int value, int extensionId, IDbTransaction transaction = null)
        {
            _logger.LogDebug($"getItemExtensionFields. siteId: {siteId}, value: {value}, extensionId: {extensionId}");
            var fieldNamesQuery = string.Format(CmdGetFieldNames, extensionId, SqlQuerySyntaxHelper.ToBoolSql(_uow.DatabaseType, true));
            var fieldNames = _uow.Connection.Query<FieldAttributeData>(fieldNamesQuery, transaction);
            var contentRowsQuery = string.Format(CmdGetContentRow, extensionId, value);
            var contentRow = _uow.Connection.Query<object>(contentRowsQuery, transaction);
            var relatedFieldsQuery = string.Format(CmdGetExtentionItems, extensionId, value);
            var relatedFields = _uow.Connection.Query<RelationExtension>(relatedFieldsQuery, transaction);
            var dict = contentRow.FirstOrDefault() as IDictionary<string, object>;

            foreach (var field in fieldNames)
            {
                field.Value = dict.FirstOrDefault(x => string.Equals(x.Key, field.FieldName, StringComparison.OrdinalIgnoreCase)).Value;
                field.RelationExtensionId = relatedFields.FirstOrDefault(x => x.FieldName == field.FieldName)?.ExtensionId;
            }

            _logger.LogDebug($"getItemExtensionFields. fieldNames: {SerializeData(fieldNames)}");

            return fieldNames.ToList();
        }

        public string GetRelatedItemName(int siteId, int id, int attributeId, IDbTransaction transaction = null)
        {
            _logger.LogDebug($"GetRelatedItemName. siteId: {siteId}, id: {id}, attributeId: {attributeId}");
            var contentAttribute = GetContentAttribute(attributeId, transaction);
            while (contentAttribute.RelatedAttributeId.HasValue)
            {
                contentAttribute = GetContentAttribute(contentAttribute.RelatedAttributeId.Value, transaction);
                if(contentAttribute.RelatedAttributeId.HasValue)
                {
                    var fieldValue = GetContentFieldValue(id, contentAttribute.ContentId, contentAttribute.AttributeName, transaction);
                    if (!int.TryParse(fieldValue, out id))
                    {
                        _logger.LogDebug($"GetRelatedItemName. result: {fieldValue}");
                        return fieldValue;
                    }
                }
            }
            var result = GetContentFieldValue(id, contentAttribute.ContentId, contentAttribute.AttributeName, transaction);
            _logger.LogDebug($"GetRelatedItemName. result: {result}");
            return result;
        }

        public Dictionary<int, string> GetManyToOneRelatedItemNames(int siteId, 
            int id, 
            int value, 
            int attributeId, 
            IDbTransaction transaction = null)
        {
            _logger.LogDebug($"GetManyToOneRelatedItemNames. siteId: {siteId}, id: {id}, value: {value}, attributeId: {attributeId}");
            var contentAttribute = GetContentAttribute(attributeId, transaction);
            if (!contentAttribute.TreeOrderField.HasValue)
            {
                _logger.LogDebug($"GetManyToOneRelatedItemNames. result: null");
                return null;
            }
            var itemId = GetContentIdByItemId(value, contentAttribute.ContentId, transaction);
            var nameAttribute = GetContentAttribute(contentAttribute.TreeOrderField.Value, transaction);
            var relatedAttribute = GetContentAttribute(id, transaction);
            var result = GetContentFieldValues(itemId, 
                nameAttribute.ContentId, 
                nameAttribute.AttributeName, 
                relatedAttribute.AttributeName, 
                transaction);
            _logger.LogDebug($"GetManyToOneRelatedItemNames. result: {SerializeData(result)}");
            return result;
        }

        private ContentAttribute GetContentAttribute(int attributeId, 
            IDbTransaction transaction)
        {
            _logger.LogDebug($"GetContentAttribute. attributeId: {attributeId}");
            var result = _uow.Connection.QuerySingleOrDefault<ContentAttribute>(CmdGetContentAttribute, 
                new { attributeId }, 
                transaction);
            _logger.LogDebug($"GetContentAttribute. result: {SerializeData(result)}");
            return result;
        }

        private string GetContentFieldValue(int id, int contentId, string attributeName, IDbTransaction transaction)
        {
            _logger.LogDebug($"GetContentFieldValue. id: {id}, contentId: {contentId}, attributeName: {attributeName}");
            var query = string.Format(CmdGetContentFieldValue, contentId, attributeName);
            var result = _uow.Connection.QuerySingleOrDefault<string>(query, new { id }, transaction);
            _logger.LogDebug($"GetContentFieldValue. result: {result}");
            return result?.ToString();
        }

        private Dictionary<int, string> GetContentFieldValues(int itemId, 
            int contentId, 
            string titleAttributeName, 
            string attributeName,
            IDbTransaction transaction)
        {
            _logger.LogDebug($"GetContentFieldValue. contentId: {contentId}, titleAttributeName: {titleAttributeName}, attributeName: {attributeName}");
            var query = string.Format(CmdGetContentFieldValues, contentId, titleAttributeName, attributeName);
            var result = _uow.Connection.Query<RelatedItem>(query, new { itemId }, transaction).ToDictionary(k => k.ContentId, v => v.Name);
            _logger.LogDebug($"GetContentFieldValue. result: {SerializeData(result)}");
            return result;
        }

        private int GetContentIdByItemId(int id, int contentId,
            IDbTransaction transaction)
        {
            _logger.LogDebug($"GetContentIdByItemId. id: {id}, contentId: {contentId}");
            var query = string.Format(CmdGetContentIdByItemId, contentId);
            var result = _uow.Connection.QuerySingleOrDefault<int>(query, new { itemId = id }, transaction);
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
