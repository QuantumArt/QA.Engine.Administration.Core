using QA.Engine.Administration.Data.Interfaces.Core.Models;
using System.Collections.Generic;

namespace QA.Engine.Administration.Data.Interfaces.Core
{
    public interface IQpDataProvider
    {
        void Publish(int siteId, int contentId, IEnumerable<AbstractItemData> items, int statusId);
    }
}
