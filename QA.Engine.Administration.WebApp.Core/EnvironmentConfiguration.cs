namespace QA.Engine.Administration.WebApp.Core
{
    public class EnvironmentConfiguration
    {
        public string CustomerCodeParamName { get; set; }
        public string SiteIdParamName { get; set; }
        public string BackendSidParamName { get; set; }
        public string HostIdParamName { get; set; }
        public int IndexOrderStep { get; set; } = 1;
        public bool IsStage { get; set; } = true;
        public bool IgnoreQPSecurityChecker { get; set; } = false;
        public bool IgnoreAuth { get; set; } = false;
    }
}
