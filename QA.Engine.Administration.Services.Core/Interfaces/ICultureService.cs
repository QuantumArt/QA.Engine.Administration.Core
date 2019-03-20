using QA.Engine.Administration.Services.Core.Models;
using System.Collections.Generic;

namespace QA.Engine.Administration.Services.Core.Interfaces
{
    public interface ICultureService
    {
        List<CultureModel> GetCultures(int siteId);
    }
}
