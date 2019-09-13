using QA.Engine.Administration.Common.Core;

namespace QA.Engine.Administration.Services.Core.Models
{
    [TypeScriptType]
    public class CustomActionModel
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string ItemIdParamName { get; set; }
        public string CultureParamName { get; set; }
        public string RegionParamName { get; set; }
    }
}
