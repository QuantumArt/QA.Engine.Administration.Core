using QA.Engine.Administration.Services.Core.Models;
using System.Collections.Generic;

namespace QA.Engine.Administration.Services.Core.Interfaces
{
    public interface ISiteMapService
    {
        List<SiteTreeModel> GetSiteMapItems(int siteId, bool isStage, int? parentId);
        List<WidgetModel> GetWidgetItems(int siteId, bool isStage, int parentId);
        List<SiteTreeModel> GetSiteMapStructure(int siteId, bool isStage);
        object PublishSiteMapItems(int siteId, bool isStage, List<int> itemIds);
    }
}
