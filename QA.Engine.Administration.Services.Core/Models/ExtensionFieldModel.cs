using QA.Engin.Administration.Common.Core;

namespace QA.Engine.Administration.Services.Core.Models
{
    [TypeScriptType]
    public class ExtensionFieldModel
    {
        public string FieldName { get; set; }
        public string TypeName { get; set; }
        public string TypeDescription { get; set; }
        public object Value { get; set; }
    }
}
