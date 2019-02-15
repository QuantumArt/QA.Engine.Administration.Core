using QA.Engine.Administration.Services.Core.Annotations;
using System.Collections.Generic;
using System.Linq;

namespace QA.Engine.Administration.Services.Core.Models
{
    [TypeScriptType]
    public class RegionModel
    {
        public int Id { get; set; }
        public string Alias { get; set; }
        public int? ParentId { get; set; }
        public string Title { get; set; }

        public List<RegionModel> Children { get; set; }

        public bool HasChildren { get { return Children?.Any() ?? false; } }

        public override string ToString()
        {
            return $"{Alias} ({Title}) Children={Children.Count()}";
        }
    }
}
