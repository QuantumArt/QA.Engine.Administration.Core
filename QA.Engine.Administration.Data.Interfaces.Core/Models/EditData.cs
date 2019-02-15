using System;
using System.Collections.Generic;
using System.Text;

namespace QA.Engine.Administration.Data.Interfaces.Core.Models
{
    public class EditData
    {
        public int ItemId { get; set; }
        public string Title { get; set; }
        public int? ExtensionId { get; set; }
        public List<ExtensionFieldData> Fields { get; set; }
    }
    
    public class ExtensionFieldData
    {
        public string FieldName { get; set; }
        public object Value { get; set; }
    }
}
