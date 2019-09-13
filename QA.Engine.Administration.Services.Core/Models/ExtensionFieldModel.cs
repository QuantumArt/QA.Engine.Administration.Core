using QA.Engine.Administration.Common.Core;

namespace QA.Engine.Administration.Services.Core.Models
{
    [TypeScriptType]
    public class ExtensionFieldModel
    {
        public string FieldName { get; set; }
        public string TypeName { get; set; }
        public string TypeDescription { get; set; }
        public object Value { get; set; }
        public int? RelationExtensionId { get; set; }
        public int AttributeId { get; set; }
    }
}
