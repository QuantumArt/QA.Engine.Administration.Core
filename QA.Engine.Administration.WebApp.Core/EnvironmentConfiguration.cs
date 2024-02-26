using System;

namespace QA.Engine.Administration.WebApp.Core
{
    public class EnvironmentConfiguration
    {
        public string CustomerCodeParamName { get; set; }

        public bool UseSameSiteNone { get; set; }
        public string SiteIdParamName { get; set; }
        public string BackendSidParamName { get; set; }
        public string HostIdParamName { get; set; }
        public int IndexOrderStep { get; set; } = 1;
        public bool UseHierarchyRegionFilter { get; set; }
        public bool IgnoreQPSecurityChecker { get; set; }
        public bool UseFake { get; set; }
        public FakeData FakeData { get; set; }
        public CustomAction CustomAction { get; set; }
        public string StartPageDiscriminator { get; set; }

        public string ConfigurationServiceUrl { get; set; }
        public string ConfigurationServiceToken { get; set; }
    }

    public class FakeData
    {
        public string DatabaseType { get; set; } = "SqlServer";
        public int UserId { get; set; }
        public string LangName { get; set; }
        public string ConnectionString { get; set; }
        public int SiteId { get; set; }
    }

    public class CustomAction
    {
        public string Alias { get; set; }
        public string ItemIdParamName { get; set; }
        public string CultureParamName { get; set; }
        public string RegionParamName { get; set; }
    }
}
