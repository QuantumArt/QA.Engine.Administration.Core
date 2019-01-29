using QA.Engine.Administration.Data.Interfaces.Core.Models;
using System.Collections.Generic;

namespace QA.Engine.Administration.Data.Interfaces.Core
{
    public interface IQpDataProvider
    {
        void Publish(int siteId, int contentId, int userId, IEnumerable<AbstractItemData> items, int statusId);
        void Reorder(int siteId, int contentId, int userId, IEnumerable<AbstractItemData> items);
        void Move(int siteId, int contentId, int userId, int itemId, int newParentId);
    }
}
