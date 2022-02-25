using QA.Engine.Administration.Common.Core;
using System.Collections.Generic;
using System.Linq;

namespace QA.Engine.Administration.Services.Core.Models
{
    [TypeScriptType]
    public class WidgetModel
    {
        public int Id { get; set; }
        public bool Archive { get; set; }
        public bool Visible { get; set; }
        
        public int? ParentId { get; set; }
        public string Alias { get; set; }
        public string Title { get; set; }
        public string ZoneName { get; set; }
        public int? ExtensionId { get; set; }
        public int? IndexOrder { get; set; }
        public bool? IsVisible { get; set; }
        public int? VersionOfId { get; set; }
        public bool Published { get; set; }
        public string Discriminator { get; set; }
        public int DiscriminatorId { get; set; }
        public string DiscriminatorTitle { get; set; }
        // public string IconUrl { get; set; }

        public List<WidgetModel> Children { get; set; }
        public List<int> RegionIds { get; set; }

        public bool HasChildren { get { return Children?.Any() ?? false; } }
        public bool HasRegions { get { return RegionIds?.Any() ?? false; } }
    }
}
