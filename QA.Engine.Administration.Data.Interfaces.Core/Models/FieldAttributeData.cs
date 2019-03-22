namespace QA.Engine.Administration.Data.Interfaces.Core.Models
{
    public class FieldAttributeData
    {
        public string FieldName { get; set; }
        public string TypeName { get; set; }
        public string TypeDescription { get; set; }
        public object Value { get; set; }
        public int? RelationExtensionId { get; set; }
        public int AttributeId { get; set; }
    }
}
