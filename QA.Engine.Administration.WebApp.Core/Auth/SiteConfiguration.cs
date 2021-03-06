﻿using Microsoft.AspNetCore.Http;
using Quantumart.QPublishing.Database;
using System;
using QP.ConfigurationService.Models;
using ConnectionInfo = QA.Engine.Administration.WebApp.Core.Business.Models.ConnectionInfo;

namespace QA.Engine.Administration.WebApp.Core.Auth
{
    public class SiteConfiguration
    {
        public string Name { get; set; }

        public string ConnectionName { get; set; }
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
            //var useHierarchyRegionsFilter = _qpSettingsService.GetSetting(connectionString, "USE_HIERARCHY_REGIONS_FILTER");
            var config = new SiteConfiguration
            {
                SiteId = siteId,
                PublishStatusImageUrl = "/Content/icons/pub.png",
                CreatedStatusImageUrl = "/Content/icons/new.jpg"
            };

            httpContext.Items[StorageKey] = config;

            return config;
        }
    }
}
