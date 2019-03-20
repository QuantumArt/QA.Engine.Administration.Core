using QA.Engine.Administration.Services.Core.Models;

namespace QA.Engine.Administration.Services.Core.Interfaces
{
    public interface ICustomActionService
    {
        CustomActionModel GetCustomAction(string alias);
    }
}
