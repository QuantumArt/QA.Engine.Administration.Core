using System.Collections.Generic;
using System.Linq;
using QA.Engine.Administration.WebApp.Core.Annotations;

namespace QA.Engine.Administration.WebApp.Core.Models
{
    [TypeScriptType]
    public class RegionViewModel
    {
        public int Id { get; set; }
        public string Alias { get; set; }
        public int? ParentId { get; set; }
        public string Title { get; set; }
        public List<RegionViewModel> Children { get; set; }
        public bool HasChildren { get { return Children?.Any() ?? false; } }
    }
}
