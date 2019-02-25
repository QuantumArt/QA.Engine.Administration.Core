using System.Collections.Generic;

namespace QA.Engine.Administration.Data.Interfaces.Core.Models
{
    public class EditData
    {
        public int ItemId { get; set; }
        public string Title { get; set; }
        public bool IsVisible { get; set; }
        public bool IsInSiteMap { get; set; }
        public int? ExtensionId { get; set; }
        public List<ExtensionFieldData> Fields { get; set; }
    }
    
    public class ExtensionFieldData
    {
        public string FieldName { get; set; }
        public object Value { get; set; }
    }
}
