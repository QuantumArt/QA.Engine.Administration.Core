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
	a.DESCRIPTION AS TypeDescription
FROM CONTENT_ATTRIBUTE ca JOIN ATTRIBUTE_TYPE a ON ca.ATTRIBUTE_TYPE_ID=a.ATTRIBUTE_TYPE_ID
WHERE ca.CONTENT_ID={0} AND ca.ATTRIBUTE_NAME <> 'ItemId' AND (ca.REQUIRED=1 OR ca.view_in_list=1)

SELECT * FROM content_{1}_united WHERE ItemId={2}
";

        public List<FieldAttributeData> GetItemExtensionFields(int siteId, int id, int extensionId)
        {
            _logger.LogDebug($"getItemExtensionFields. siteId: {siteId}, id: {id}, extensionId: {extensionId}");
            var query = string.Format(CmdGetExtantionItems, extensionId, extensionId, id);
            using (var multi = _connection.QueryMultiple(query))
            {
                var fieldNames = multi.Read<FieldAttributeData>().ToList();
                var contentRow = multi.ReadFirstOrDefault<object>();

                var dict = contentRow as IDictionary<string, object>;

                foreach(var field in fieldNames)
                    field.Value = dict.Where(x => x.Key == field.FieldName).FirstOrDefault().Value;

                _logger.LogDebug($"getItemExtensionFields. fieldNames: {Newtonsoft.Json.JsonConvert.SerializeObject(fieldNames)}");

                return fieldNames;
            }
        }
    }
}
