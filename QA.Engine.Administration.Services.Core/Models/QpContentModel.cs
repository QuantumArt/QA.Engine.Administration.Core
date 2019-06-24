using QA.Engine.Administration.Common.Core;
using System.Collections.Generic;

namespace QA.Engine.Administration.Services.Core.Models
{
    [TypeScriptType]
    public class QpContentModel
    {
        public int Id { get; set; }
        public List<QpFieldModel> Fields { get; set; }
    }
}
