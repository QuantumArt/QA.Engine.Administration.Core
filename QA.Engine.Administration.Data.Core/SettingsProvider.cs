using Dapper;
using QA.DotNetCore.Engine.Persistent.Interfaces;
using QA.Engine.Administration.Data.Core.Qp;
using QA.Engine.Administration.Data.Interfaces.Core;
using QA.Engine.Administration.Data.Interfaces.Core.Models;
using System.Data;
using System.Linq;
using NLog;

namespace QA.Engine.Administration.Data.Core
{
    public class SettingsProvider : ISettingsProvider
    {
        private readonly IMetaInfoRepository _metaInfoRepository;
        private readonly IQpMetadataManager _qpMetadataManager;
        private readonly IQpContentManager _qpContentManager;
        private readonly IQpDbConnector _qpDbConnector;
        private readonly IUnitOfWork _uow;
        private readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        private string AbstractItemNetName => "QPAbstractItem";
        private string RegionNetName => "QPRegion";

        public SettingsProvider(
            IMetaInfoRepository metaInfoRepository, IQpMetadataManager qpMetadataManager, IQpContentManager qpContentManager,
            IUnitOfWork uow, IQpDbConnector qpDbConnector)
        {
            _metaInfoRepository = metaInfoRepository;
            _qpMetadataManager = qpMetadataManager;
            _qpContentManager = qpContentManager;
            _qpDbConnector = qpDbConnector;
            _uow = uow;
        }

        public int GetContentId(int siteId, IDbTransaction transaction = null)
        {
            _logger.ForDebugEvent().Message("GetContentId").Property("siteId", siteId).Log();
            var content = _metaInfoRepository.GetContent(AbstractItemNetName, siteId);
            _logger.ForDebugEvent().Message("GetContentId").Property("contentId", content.ContentId).Log();
            return content.ContentId;
        }

        public bool HasRegion(int siteId, IDbTransaction transaction = null)
        {
            _logger.ForDebugEvent().Message("HasRegion").Property("siteId", siteId).Log();
            var content = _metaInfoRepository.GetContent(RegionNetName, siteId);
            var hasRegion = content != null;
            _logger.ForDebugEvent().Message("HasRegion").Property("hasRegion", hasRegion).Log();
            return hasRegion;
        }

        public QpContentData GetContent(int siteId, string contentName, IDbTransaction transaction = null)
        {
            _logger.ForDebugEvent().Message("GetContent")
                .Property("siteId", siteId)
                .Property("contentName", contentName)
                .Log();
            var siteName = _qpMetadataManager.GetSiteName(siteId);
            var contents = _qpContentManager.Connect()
                    .SiteName(siteName)
                    .ContentName("CONTENT")
                    .Fields("CONTENT_ID, CONTENT_NAME, NET_CONTENT_NAME")
                    .Where($"NET_CONTENT_NAME = '{contentName}'")
                    .GetRealData();

            var result = contents.PrimaryContent.Select()
                .Select(x => new QpContentData
                {
                    Id = int.Parse(x["CONTENT_ID"].ToString() ?? "0"),
                    Name = x["NET_CONTENT_NAME"].ToString()
                }).FirstOrDefault();

            _logger.ForDebugEvent().Message("GetContent")
                .Property("contentId", result?.Id ?? 0)
                .Property("contentName", result?.Name ?? "");

            return result;
        }

        public string GetIconUrl(int siteId, IDbTransaction transaction = null)
        {
            _logger.ForDebugEvent().Message("GetIconUrl").Property("siteId", siteId).Log();
            var fieldId = _qpDbConnector.DbConnector.GetAttributeIdByNetNames(siteId, "QPDiscriminator", "IconUrl");
            var url = _qpDbConnector.DbConnector.GetUrlForFileAttribute(fieldId, true, false);
            _logger.ForDebugEvent().Message("GetIconUrl").Property("url", url).Log();
            return url;
        }

        public CustomActionData GetCustomAction(string alias, IDbTransaction transaction = null)
        {
            _logger.ForDebugEvent().Message("GetCustomAction").Property("alias", alias).Log();
            var query = $"SELECT c.ID as Id, b.CODE as Code FROM CUSTOM_ACTION c JOIN BACKEND_ACTION b ON c.ACTION_ID=b.ID WHERE ALIAS='{alias.ToLower()}'";
            var result = _uow.Connection.QuerySingleOrDefault<CustomActionData>(query);
            _logger.ForDebugEvent().Message("GetCustomAction").Property("result", result).Log();            
            return result;
        }
    }
}
