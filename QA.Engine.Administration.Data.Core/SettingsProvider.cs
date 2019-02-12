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

        private string AbstractItemNetName => "QPAbstractItem";
        private string RegionNetName => "QPRegion";

        public SettingsProvider(IMetaInfoRepository metaInfoRepository, IQpMetadataManager qpMetadataManager, IQpContentManager qpContentManager)
        {
            _metaInfoRepository = metaInfoRepository;
            _qpMetadataManager = qpMetadataManager;
            _qpContentManager = qpContentManager;
        }

        public int GetContentId(int siteId)
        {
            var content = _metaInfoRepository.GetContent(AbstractItemNetName, siteId);
            return content.ContentId;
        }

        public bool HasRegion(int siteId)
        {
            var content = _metaInfoRepository.GetContent(RegionNetName, siteId);
            return content != null;
        }

        public QpContentData GetContent(int siteId, string contentName)
        {
            var siteName = _qpMetadataManager.GetSiteName(siteId);
            var contents = _qpContentManager
                      .Connect()
                      .SiteName(siteName)
                      .ContentName("CONTENT")
                      .Fields("CONTENT_ID, CONTENT_NAME, NET_CONTENT_NAME")
                      .Where($"[NET_CONTENT_NAME] = '{contentName}'")
                      .GetRealData();

            var result = contents.PrimaryContent.Select()
                .Select(x => new QpContentData
                {
                    Id = int.Parse(x["CONTENT_ID"].ToString()),
                    Name = x["NET_CONTENT_NAME"].ToString()
                }).ToList();

            return result.FirstOrDefault();
        }

        public List<QpFieldData> GetFields(int siteId, int contentId)
        {
            var siteName = _qpMetadataManager.GetSiteName(siteId);
            var fields = _qpContentManager
                .Connect()
                .SiteName(siteName)
                .ContentName("CONTENT_ATTRIBUTE")
                .Fields("ATTRIBUTE_ID, CONTENT_ID, ATTRIBUTE_NAME, NET_ATTRIBUTE_NAME")
                .Where($"[NET_ATTRIBUTE_NAME] is not null AND CONTENT_ID = {contentId}")
                .GetRealData();

            var result = fields.PrimaryContent.Select()
                .Select(x => new QpFieldData
                {
                    Id = int.Parse(x["ATTRIBUTE_ID"].ToString()),
                    Name = x["NET_ATTRIBUTE_NAME"].ToString()
                }).ToList();

            return result;
        }
    }
}
