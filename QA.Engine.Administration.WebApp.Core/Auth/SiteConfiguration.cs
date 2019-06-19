using Microsoft.AspNetCore.Http;
using Quantumart.QPublishing.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using QA.Engine.Administration.WebApp.Core.Business.Models;
using ConnectionInfo = QA.Engine.Administration.WebApp.Core.Business.Models.ConnectionInfo;

namespace QA.Engine.Administration.WebApp.Core.Auth
{
    public class SiteConfiguration
    {
        public string Name { get; set; }

        public string ConnectionName { get; set; }
        public ConnectionInfo ConnectionInfo { get; set; }

        public string SiteDescription { get; set; }

        public int SiteId { get; set; }

        public string ContainerName { get; set; }

        public string PublishStatusImageUrl { get; set; }

        public string CreatedStatusImageUrl { get; set; }

        public bool IsEmptyRegionListIfNotSelected { get; set; }

        public bool UseHierarchyRegionsFilter { get; set; }

        //public int RootPageId
        //{
        //    get
        //    {
        //        var result = ClientUtils.Resolve<ISiteMapService>().GetRootPage();
        //        if (!result.IsSucceeded)
        //        {
        //            Throws.Exception(result.Error.Message);
        //        }

        //        return result.Result?.Id ?? 0;
        //    }
        //}

        private const string StorageKey = "CurrentSiteConfiguration";

        public static SiteConfiguration Get(HttpContext httpContext)
        {
            if (httpContext == null)
                throw new Exception(string.Format("Неверный файл конфигурации или конфигурация не найдена(name = {0}).", string.Empty));

            //if (httpContext.Items[StorageKey] == null)
            //    throw new Exception(string.Format("Неверный файл конфигурации или конфигурация не найдена(name = {0}).", string.Empty));

            return (SiteConfiguration)httpContext.Items[StorageKey];
        }

        public static SiteConfiguration Set(HttpContext httpContext, string customerCode, int siteId, bool useFake)
        {
            var connectionInfo = GetConnectionInfo(customerCode, useFake);
            //var useHierarchyRegionsFilter = _qpSettingsService.GetSetting(connectionString, "USE_HIERARCHY_REGIONS_FILTER");
            var config = new SiteConfiguration
            {
                // UseHierarchyRegionsFilter = useHierarchyRegionsFilter != null && useHierarchyRegionsFilter.ToLower() == "true",
                ConnectionInfo = connectionInfo,
                SiteId = siteId,
                PublishStatusImageUrl = "/Content/icons/pub.png",
                CreatedStatusImageUrl = "/Content/icons/new.jpg",
            };

            httpContext.Items[StorageKey] = config;

            return config;
        }

        private static ConnectionInfo GetConnectionInfo(string customerCode, bool useFake)
        {
            if (useFake)
                return null;
            XmlNode xmlNode = DBConnector.GetQpConfig().SelectSingleNode("configuration/customers/customer[@customer_name='" + customerCode + "']/db/text()");
            if (xmlNode != null)
            {
                string database = "MsSql";
                string connectionString = xmlNode.Value;
                var databaseNode = xmlNode.Attributes?["database"];
                if (databaseNode != null)
                {
                    database = databaseNode.Value;
                }

                if (database == "MsSql")
                {
                    connectionString = connectionString.Replace("Provider=SQLOLEDB;", "");
                }
                return new ConnectionInfo
                {
                    ConnectionString = connectionString,
                    DatabaseType = database
                };
            }

            throw new InvalidOperationException("Cannot load connection string from QP7 configuration file");
        }
    }
}
