using Quantumart.QPublishing.Database;
using Quantumart.QPublishing.Info;
using System.Collections.Generic;

namespace QA.Engine.Administration.Data.Core.Qp
{
    public interface IQpMetadataManager
    {
        List<ContentAttribute> GetContentAttributes(string siteName, string contentName);

        List<ContentAttribute> GetContentAttributes(int contentId);

        ContentAttribute GetContentAttribute(string siteName, string contentName, string fieldName);

        int GetContentId(string siteName, string contentName);

        string GetContentName(int contentId);

        int GetSiteId(string siteName);

        string GetSiteName(int siteId);
    }
}
