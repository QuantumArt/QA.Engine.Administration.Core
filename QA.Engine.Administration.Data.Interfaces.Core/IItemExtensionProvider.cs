using QA.Engine.Administration.Data.Interfaces.Core.Models;
using System.Collections.Generic;
using System.Data;

namespace QA.Engine.Administration.Data.Interfaces.Core
{
    public interface IItemExtensionProvider
    {
        List<FieldAttributeData> GetItemExtensionFields(int siteId, int id, int extantionId, IDbTransaction transaction = null);
        string GetRelatedItemName(int siteId, int id, int attributeId, IDbTransaction transaction = null);
        Dictionary<int, string> GetManyToOneRelatedItemNames(int siteId, int id, int value, int attributeId, IDbTransaction transaction = null);
    }
}
