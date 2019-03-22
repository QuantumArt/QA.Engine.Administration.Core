using Dapper;
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
        private readonly IDbConnection _connection;
        private readonly INetNameQueryAnalyzer _netNameQueryAnalyzer;
        private readonly IMetaInfoRepository _metaInfoRepository;
        private readonly ILogger<ItemExtensionProvider> _logger;

        public ItemExtensionProvider(IUnitOfWork uow, INetNameQueryAnalyzer netNameQueryAnalyzer, IMetaInfoRepository metaInfoRepository, ILogger<ItemExtensionProvider> logger)
        {
            _connection = uow.Connection;
            _netNameQueryAnalyzer = netNameQueryAnalyzer;
            _metaInfoRepository = metaInfoRepository;
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
FROM CONTENT_ATTRIBUTE a JOIN (SELECT ATTRIBUTE_ID, ATTRIBUTE_NAME, RELATED_ATTRIBUTE_ID FROM CONTENT_ATTRIBUTE WHERE CONTENT_ID={0} AND RELATED_ATTRIBUTE_ID is not null) b
	ON a.ATTRIBUTE_ID=b.RELATED_ATTRIBUTE_ID
";
        private const string CmdGetContentAttribute = @"
SELECT
    ca.ATTRIBUTE_ID AS AttributeId,
    ca.CONTENT_ID AS ContentId,
    ca.ATTRIBUTE_NAME AS AttributeName,
    ca.RELATED_ATTRIBUTE_ID AS RelatedAttributeId,
    LOWER(t.TYPE_NAME) AS AttributeType
FROM CONTENT_ATTRIBUTE ca JOIN ATTRIBUTE_TYPE t ON ca.ATTRIBUTE_TYPE_ID=t.ATTRIBUTE_TYPE_ID
WHERE ca.ATTRIBUTE_ID=@attributeId
";
        private const string CmdGetContentFieldValue = @"
SELECT {1}
FROM content_{0}_united c
WHERE CONTENT_ITEM_ID=@id
";

        public List<FieldAttributeData> GetItemExtensionFields(int siteId, int id, int extensionId)
        {
            _logger.LogDebug($"getItemExtensionFields. siteId: {siteId}, id: {id}, extensionId: {extensionId}");
            var query = string.Format(CmdGetExtantionItems, extensionId, id);
            using (var multi = _connection.QueryMultiple(query))
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
                    var fieldValue = GetContentFieldValue(id, contentAttribute);
                    if (!int.TryParse(fieldValue, out id))
                    {
                        _logger.LogDebug($"GetRelatedItemName. result: {fieldValue}");
                        return fieldValue;
                    }
                }
            }
            var result = GetContentFieldValue(id, contentAttribute);
            _logger.LogDebug($"GetRelatedItemName. result: {result}");
            return result;
        }

        private ContentAttribute GetContentAttribute(int attributeId)
        {
            _logger.LogDebug($"GetContentAttribute. attributeId: {attributeId}");
            var result = _connection.QuerySingleOrDefault<ContentAttribute>(CmdGetContentAttribute, new { attributeId });
            _logger.LogDebug($"GetContentAttribute. result: {SerializeData(result)}");
            return result;
        }

        private string GetContentFieldValue(int id, ContentAttribute contentAttribute)
        {
            _logger.LogDebug($"GetContentFieldValue. attributeId: {SerializeData(contentAttribute)}");
            var query = string.Format(CmdGetContentFieldValue, contentAttribute.ContentId, contentAttribute.AttributeName);
            var result = _connection.QuerySingleOrDefault<string>(query, new { id });
            _logger.LogDebug($"GetContentFieldValue. result: {result}");
            return result?.ToString();
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
        }
    }
}
