using QA.Engine.Administration.Data.Interfaces.Core.Models;
using System.Collections.Generic;
using System.Data;

namespace QA.Engine.Administration.Data.Interfaces.Core
{
    public interface ISettingsProvider
    {
        int GetContentId(int siteId, IDbTransaction transaction = null);
        bool HasRegion(int siteId, IDbTransaction transaction = null);
        QpContentData GetContent(int siteId, string contentName, IDbTransaction transaction = null);
        string GetIconUrl(int siteId, IDbTransaction transaction = null);
        CustomActionData GetCustomAction(string alias, IDbTransaction transaction = null);
    }
}
