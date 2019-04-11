using QA.Engin.Administration.Common.Core;
using System.Collections.Generic;
using System.Linq;

namespace QA.Engine.Administration.Services.Core.Models
{
    [TypeScriptType]
    public class PageModel
    {
        public int Id { get; set; }
        public bool IsArchive { get; set; }
        public int? ParentId { get; set; }
        public string Alias { get; set; }
        public string Title { get; set; }
        public string ZoneName { get; set; }
        public int? ExtensionId { get; set; }
        public int? IndexOrder { get; set; }
        public bool? IsVisible { get; set; }
        public int? VersionOfId { get; set; }
        public bool Published { get; set; }
        public bool? IsInSiteMap { get; set; }
        public string Discriminator { get; set; }
        public int DiscriminatorId { get; set; }
        public string DiscriminatorTitle { get; set; }
        // public string IconUrl { get; set; }

        public List<WidgetModel> Widgets { get; set; }
        public List<PageModel> Children { get; set; }
        public List<PageModel> ContentVersions { get; set; }
        public List<int> RegionIds { get; set; }

        public bool HasWidgets { get { return Widgets?.Any() ?? false; } }
        public bool HasChildren { get { return Children?.Any() ?? false; } }
        public bool HasContentVersion { get { return ContentVersions?.Any() ?? false; } }
        public bool HasRegions { get { return RegionIds?.Any() ?? false; } }
    }
}
