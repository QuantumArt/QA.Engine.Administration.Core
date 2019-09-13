namespace QA.Engine.Administration.WebApp.Core
{
    public class EnvironmentConfiguration
    {
        public string ServiceName { get; set; }
        public string ServiceVersion { get; set; }
        public string CustomerCodeParamName { get; set; }
        public string SiteIdParamName { get; set; }
        public string BackendSidParamName { get; set; }
        public string HostIdParamName { get; set; }
        public int IndexOrderStep { get; set; } = 1;
        public bool UseHierarchyRegionFilter { get; set; }
        public bool IgnoreQPSecurityChecker { get; set; } = false;
        public bool UseFake { get; set; } = false;
        public FakeData FakeData { get; set; }
        public CustomAction CustomAction { get; set; }
        public string StartPageDiscriminator { get; set; }
        public string DatabaseType { get; set; }

        public string ConfigurationServiceUrl { get; set; }
        public string ConfigurationServiceToken { get; set; }
    }

    public class FakeData
    {
        public int UserId { get; set; }
        public string LangName { get; set; }
    }

    public class CustomAction
    {
        public string Alias { get; set; }
        public string ItemIdParamName { get; set; }
        public string CultureParamName { get; set; }
        public string RegionParamName { get; set; }
    }
}
