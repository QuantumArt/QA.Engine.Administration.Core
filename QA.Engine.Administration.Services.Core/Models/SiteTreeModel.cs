using System.Collections.Generic;
using System.Linq;

namespace QA.Engine.Administration.Services.Core.Models
{
    public class SiteTreeModel
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
        public bool IsPage { get; set; }
        public int? VersionOfId { get; set; }
        public bool Published { get; set; }
        public bool? IsInSiteMap { get; set; }
        public int DiscriminatorId { get; set; }

        public List<WidgetTreeModel> Widgets { get; set; }
        public List<SiteTreeModel> Children { get; set; }
        public List<SiteTreeModel> ContentVersions { get; set; }
        public DiscriminatorModel Discriminator { get; set; }

        public bool HasWidgets { get { return Widgets?.Any() ?? false; } }
        public bool HasChildren { get { return Children?.Any() ?? false; } }
        public bool HasContentVersion { get { return ContentVersions?.Any() ?? false; } }
    }
}
