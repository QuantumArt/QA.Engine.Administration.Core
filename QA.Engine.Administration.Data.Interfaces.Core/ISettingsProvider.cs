using QA.Engine.Administration.Data.Interfaces.Core.Models;
using System.Collections.Generic;

namespace QA.Engine.Administration.Data.Interfaces.Core
{
    public interface ISettingsProvider
    {
        int GetContentId(int siteId);
        bool HasRegion(int siteId);
        QpContentData GetContent(int siteId, string contentName);
        List<QpFieldData> GetFields(int siteId, int contentId);
        string GetIconUrl(int siteId);
        CustomActionData GetCustomAction(string alias);
    }
}
