using QA.Engine.Administration.Services.Core.Models;

namespace QA.Engine.Administration.Services.Core.Interfaces
{
    public interface IContentService
    {
        QpContentModel GetQpContent(int siteId, string contentName);
    }
}
