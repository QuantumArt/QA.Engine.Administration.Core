using System;
using Dapper;
using QA.DotNetCore.Engine.Persistent.Interfaces;
using QA.Engine.Administration.Data.Interfaces.Core;
using QA.Engine.Administration.Data.Interfaces.Core.Models;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using NLog;
using QA.DotNetCore.Engine.Persistent.Dapper;

namespace QA.Engine.Administration.Data.Core
{
    public class ItemExtensionProvider: IItemExtensionProvider
    {
        private readonly IUnitOfWork _uow;
        private readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        public ItemExtensionProvider(IUnitOfWork uow)
        {
            _uow = uow;
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
        private const string CmdGetExtensionItems = @"
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
            _logger.ForDebugEvent().Message("GetItemExtensionFields")
                .Property("siteId", siteId)
                .Property("value", value)
                .Property("extensionId", extensionId)
                .Log();
            var fieldNamesQuery = string.Format(CmdGetFieldNames, extensionId, SqlQuerySyntaxHelper.ToBoolSql(_uow.DatabaseType, true));
            var fieldNames = _uow.Connection.Query<FieldAttributeData>(fieldNamesQuery, transaction).ToArray();
            var contentRowsQuery = string.Format(CmdGetContentRow, extensionId, value);
            var contentRow = _uow.Connection.Query<object>(contentRowsQuery, transaction);
            var relatedFieldsQuery = string.Format(CmdGetExtensionItems, extensionId);
            var relatedFields = _uow.Connection.Query<RelationExtension>(relatedFieldsQuery, transaction).ToArray();
            var dict = contentRow.FirstOrDefault() as IDictionary<string, object>;

            foreach (var field in fieldNames)
            {
                field.Value = dict?.FirstOrDefault(x => string.Equals(x.Key, field.FieldName, StringComparison.OrdinalIgnoreCase)).Value;
                field.RelationExtensionId = relatedFields.FirstOrDefault(x => x.FieldName == field.FieldName)?.ExtensionId;
            }

            _logger.ForDebugEvent().Message("GetItemExtensionFields")
                .Property("fieldNames", fieldNames)
                .Log();

            return fieldNames.ToList();
        }

        public string GetRelatedItemName(int siteId, int id, int attributeId, IDbTransaction transaction = null)
        {
            _logger.ForDebugEvent().Message("GetRelatedItemName")
                .Property("siteId", siteId)
                .Property("id", id)
                .Property("attributeId", attributeId)
                .Log();
            
            var contentAttribute = GetContentAttribute(attributeId, transaction);
            while (contentAttribute.RelatedAttributeId.HasValue)
            {
                contentAttribute = GetContentAttribute(contentAttribute.RelatedAttributeId.Value, transaction);
                if(contentAttribute.RelatedAttributeId.HasValue)
                {
                    var fieldValue = GetContentFieldValue(id, contentAttribute.ContentId, contentAttribute.AttributeName, transaction);
                    if (!int.TryParse(fieldValue, out id))
                    {
                        _logger.ForDebugEvent().Message("GetRelatedItemName").Property("result", fieldValue).Log();                        
                        return fieldValue;
                    }
                }
            }
            var result = GetContentFieldValue(id, contentAttribute.ContentId, contentAttribute.AttributeName, transaction);
            _logger.ForDebugEvent().Message("GetRelatedItemName").Property("result", result).Log();
            return result;
        }

        public Dictionary<int, string> GetManyToOneRelatedItemNames(int siteId, 
            int id, 
            int value, 
            int attributeId, 
            IDbTransaction transaction = null)
        {
            _logger.ForDebugEvent().Message("GetManyToOneRelatedItemNames")
                .Property("siteId", siteId)
                .Property("id", id)
                .Property("attributeId", attributeId)
                .Log();
            
            Dictionary<int, string> result;
            var contentAttribute = GetContentAttribute(attributeId, transaction);
            if (!contentAttribute.TreeOrderField.HasValue)
            {
                result = null;
            }
            else
            {
                var itemId = GetContentIdByItemId(value, contentAttribute.ContentId, transaction);
                var nameAttribute = GetContentAttribute(contentAttribute.TreeOrderField.Value, transaction);
                var relatedAttribute = GetContentAttribute(id, transaction);
                result = GetContentFieldValues(itemId, 
                    nameAttribute.ContentId, 
                    nameAttribute.AttributeName, 
                    relatedAttribute.AttributeName, 
                    transaction);
            }
            _logger.ForDebugEvent().Message("GetRelatedItemName").Property("result", result).Log();                
            return result;
        }

        private ContentAttribute GetContentAttribute(int attributeId, 
            IDbTransaction transaction)
        {
            _logger.ForDebugEvent().Message("GetContentAttribute").Property("attributeId", attributeId).Log();
            var result = _uow.Connection.QuerySingleOrDefault<ContentAttribute>(CmdGetContentAttribute, 
                new { attributeId }, 
                transaction);
            _logger.ForDebugEvent().Message("GetContentAttribute").Property("result", result).Log();
            return result;
        }

        private string GetContentFieldValue(int id, int contentId, string attributeName, IDbTransaction transaction)
        {
            _logger.ForDebugEvent().Message("GetManyToOneRelatedItemNames")
                .Property("contentId", contentId)
                .Property("id", id)
                .Property("attributeName", attributeName)
                .Log();
            
            var query = string.Format(CmdGetContentFieldValue, contentId, attributeName);
            var result = _uow.Connection.QuerySingleOrDefault<string>(query, new { id }, transaction);
            _logger.ForDebugEvent().Message("GetContentFieldValue").Property("result", result).Log();
            return result;
        }

        private Dictionary<int, string> GetContentFieldValues(int itemId, 
            int contentId, 
            string titleAttributeName, 
            string attributeName,
            IDbTransaction transaction)
        {
            _logger.ForDebugEvent().Message("GetManyToOneRelatedItemNames")
                .Property("contentId", contentId)
                .Property("titleAttributeName", titleAttributeName)
                .Property("attributeName", attributeName)
                .Log();
            
            var query = string.Format(CmdGetContentFieldValues, contentId, titleAttributeName, attributeName);
            var result = _uow.Connection.Query<RelatedItem>(query, new { itemId }, transaction).ToDictionary(k => k.ContentId, v => v.Name);
            _logger.ForDebugEvent().Message("GetContentFieldValues").Property("result", result).Log();
            return result;
        }

        private int GetContentIdByItemId(int id, int contentId,
            IDbTransaction transaction)
        {
            _logger.ForDebugEvent().Message("GetManyToOneRelatedItemNames")
                .Property("contentId", contentId)
                .Property("id", id)
                .Log();
            
            var query = string.Format(CmdGetContentIdByItemId, contentId);
            var result = _uow.Connection.QuerySingleOrDefault<int>(query, new { itemId = id }, transaction);
            _logger.ForDebugEvent().Message("GetContentIdByItemId").Property("result", result).Log();
            return result;
        }

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
