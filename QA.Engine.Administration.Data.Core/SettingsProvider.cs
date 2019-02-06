using QA.DotNetCore.Engine.Persistent.Interfaces;
using QA.Engine.Administration.Data.Interfaces.Core;

namespace QA.Engine.Administration.Data.Core
{
    public class SettingsProvider : ISettingsProvider
    {
        private readonly IMetaInfoRepository _metaInfoRepository;

        private string AbstractItemNetName => "QPAbstractItem";
        private string RegionNetName => "QPRegion";

        public SettingsProvider(IMetaInfoRepository metaInfoRepository)
        {
            _metaInfoRepository = metaInfoRepository;
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
    }
}
