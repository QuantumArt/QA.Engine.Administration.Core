using QP.ConfigurationService.Models;

namespace QA.Engine.Administration.WebApp.Core.Business.Models
{
    public class ConnectionInfo
    {
        public DatabaseType DatabaseType { get; set; }
        public string ConnectionString { get; set; }
    }
}
