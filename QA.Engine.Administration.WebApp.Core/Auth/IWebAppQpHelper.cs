using QP.ConfigurationService.Models;

namespace QA.Engine.Administration.WebApp.Core.Auth
{
    public interface IWebAppQpHelper
    {
        /// <summary>
        /// Id бэкенда
        /// </summary>
        string BackendSid { get; }

        CustomerConfiguration GetCurrentCustomerConfiguration();

        /// <summary>
        /// Id хоста
        /// </summary>
        string HostId { get; }
        /// <summary>
        /// Признак запуска через Custom Action Qp
        /// </summary>
        bool IsQpMode { get; }
        /// <summary>
        /// Ключ
        /// </summary>
        string QpKey { get; }
        /// <summary>
        /// Идентификатор сайта
        /// </summary>
        int SiteId { get; }

        int SavedSiteId { get; }

        string SavedConnectionString { get; }

        DatabaseType SavedDbType { get; }
        /// <summary>
        /// Id пользователя
        /// </summary>
        int UserId { get; }
        string CustomerCode { get; }
    }
}
