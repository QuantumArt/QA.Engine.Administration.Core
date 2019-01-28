using System.ComponentModel;

namespace QA.Engine.Administration.WebApp.Core.Auth
{
    public enum QpLanguage : byte
    {
        [Description("ru-RU")]
        Default = 0,

        [Description("en-US")]
        English = 1,

        [Description("ru-RU")]
        Russian = 2
    }
}
