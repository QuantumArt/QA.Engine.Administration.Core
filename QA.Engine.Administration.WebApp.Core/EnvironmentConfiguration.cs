namespace QA.Engine.Administration.WebApp.Core
{
    public class EnvironmentConfiguration
    {
        public string CustomerCodeParamName { get; set; }
        public string SiteIdParamName { get; set; }
        public string BackendSidParamName { get; set; }
        public string HostIdParamName { get; set; }
        public int IndexOrderStep { get; set; } = 1;
        public bool UseHierarchyRegionFilter { get; set; }
        public bool IgnoreQPSecurityChecker { get; set; } = false;
        public bool UseFake { get; set; } = false;
        public FakeData FakeData { get; set; }
    }

    public class FakeData
    {
        public int UserId { get; set; }
        public string LangName { get; set; }
    }
}
