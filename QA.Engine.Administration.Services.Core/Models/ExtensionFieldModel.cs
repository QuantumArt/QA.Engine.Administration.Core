using QA.Engine.Administration.Services.Core.Annotations;
using System;
using System.Collections.Generic;
using System.Text;

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
