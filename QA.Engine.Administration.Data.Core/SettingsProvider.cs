using Dapper;
using Microsoft.Extensions.Logging;
using QA.DotNetCore.Engine.Persistent.Interfaces;
using QA.Engine.Administration.Data.Core.Qp;
using QA.Engine.Administration.Data.Interfaces.Core;
using QA.Engine.Administration.Data.Interfaces.Core.Models;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace QA.Engine.Administration.Data.Core
{
    public class SettingsProvider : ISettingsProvider
    {
        private readonly IMetaInfoRepository _metaInfoRepository;
        private readonly IQpMetadataManager _qpMetadataManager;
        private readonly IQpContentManager _qpContentManager;
        private readonly IQpDbConnector _qpDbConnector;
        private readonly IUnitOfWork _uow;
        private readonly ILogger<SettingsProvider> _logger;

        private string AbstractItemNetName => "QPAbstractItem";
        private string RegionNetName => "QPRegion";

        public SettingsProvider(
            IMetaInfoRepository metaInfoRepository, IQpMetadataManager qpMetadataManager, IQpContentManager qpContentManager,
            IUnitOfWork uow, IQpDbConnector qpDbConnector, ILogger<SettingsProvider> logger)
        {
            _metaInfoRepository = metaInfoRepository;
            _qpMetadataManager = qpMetadataManager;
            _qpContentManager = qpContentManager;
            _qpDbConnector = qpDbConnector;
            _uow = uow;
            _logger = logger;
        }

        public int GetContentId(int siteId)
        {
            _logger.LogDebug($"getContent. siteId: {siteId}");
            var content = _metaInfoRepository.GetContent(AbstractItemNetName, siteId);
            _logger.LogDebug($"getContent. contentId: {content.ContentId}");
            return content.ContentId;
        }

        public bool HasRegion(int siteId)
        {
            _logger.LogDebug($"hasRegion. siteId: {siteId}");
            var content = _metaInfoRepository.GetContent(RegionNetName, siteId);
            _logger.LogDebug($"hasRegion. hasregion: {content != null}");
            return content != null;
        }

        public QpContentData GetContent(int siteId, string contentName)
        {
            _logger.LogDebug($"getContent. siteId: {siteId}, contentName: {contentName}");
            var siteName = _qpMetadataManager.GetSiteName(siteId);
            var contents = _qpContentManager
                      .Connect()
                      .SiteName(siteName)
                      .ContentName("CONTENT")
                      .Fields("CONTENT_ID, CONTENT_NAME, NET_CONTENT_NAME")
                      .Where($"NET_CONTENT_NAME = '{contentName}'")
                      .GetRealData();

            var result = contents.PrimaryContent.Select()
                .Select(x => new QpContentData
                {
                    Id = int.Parse(x["CONTENT_ID"].ToString()),
                    Name = x["NET_CONTENT_NAME"].ToString()
                }).ToList();

            _logger.LogDebug($"getContent. contentId: {result.FirstOrDefault()?.Id}, contentName: {result.FirstOrDefault()?.Name }");

            return result.FirstOrDefault();
        }

        public List<QpFieldData> GetFields(int siteId, int contentId)
        {
            _logger.LogDebug($"getFields. siteId: {siteId}, contentId: {contentId}");
            var siteName = _qpMetadataManager.GetSiteName(siteId);
            var fields = _qpContentManager
                .Connect()
                .SiteName(siteName)
                .ContentName("CONTENT_ATTRIBUTE")
                .Fields("ATTRIBUTE_ID, CONTENT_ID, ATTRIBUTE_NAME, NET_ATTRIBUTE_NAME")
                .Where($"NET_ATTRIBUTE_NAME is not null AND CONTENT_ID = {contentId}")
                .GetRealData();

            var result = fields.PrimaryContent.Select()
                .Select(x => new QpFieldData
                {
                    Id = int.Parse(x["ATTRIBUTE_ID"].ToString()),
                    Name = x["NET_ATTRIBUTE_NAME"].ToString()
                }).ToList();

            _logger.LogDebug($"getFields. fields: {Newtonsoft.Json.JsonConvert.SerializeObject(result)}");

            return result;
        }

        public string GetIconUrl(int siteId)
        {
            _logger.LogDebug($"getIconUrl. siteId: {siteId}");
            var fieldId = _qpDbConnector.DbConnector.GetAttributeIdByNetNames(siteId, "QPDiscriminator", "IconUrl");
            var url = _qpDbConnector.DbConnector.GetUrlForFileAttribute(fieldId, true, false);
            _logger.LogDebug($"getIconUrl. url: {url}");
            return url;
        }

        public CustomActionData GetCustomAction(string alias)
        {
            _logger.LogDebug($"getCustomActionCode. alias: {alias}");
            var query = $"SELECT c.ID as Id, b.CODE as Code FROM CUSTOM_ACTION c JOIN BACKEND_ACTION b ON c.ACTION_ID=b.ID WHERE ALIAS='{alias.ToLower()}'";
            var result = _uow.Connection.QuerySingleOrDefault<CustomActionData>(query);
            _logger.LogDebug($"getCustomActionCode. result: {Newtonsoft.Json.JsonConvert.SerializeObject(result)}");
            return result;
        }
    }
}
