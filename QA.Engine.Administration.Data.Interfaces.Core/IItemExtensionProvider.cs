using QA.Engine.Administration.Data.Interfaces.Core.Models;
using System.Collections.Generic;

namespace QA.Engine.Administration.Data.Interfaces.Core
{
    public interface IItemExtensionProvider
    {
        List<FieldAttributeData> GetItemExtensionFields(int siteId, int id, int extantionId);
    }
}
