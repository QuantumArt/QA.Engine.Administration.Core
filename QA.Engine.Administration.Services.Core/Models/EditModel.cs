using QA.Engine.Administration.Services.Core.Annotations;
using System.Collections.Generic;

namespace QA.Engine.Administration.Services.Core.Models
{
    [TypeScriptType]
    public class EditModel
    {
        public int ItemId { get; set; }
        public string Title { get; set; }
        public bool IsVisible { get; set; }
        public bool IsInSiteMap { get; set; }
        public int? ExtensionId { get; set; }
        public List<ExtensionFieldModel> Fields { get; set; }
    }
}
