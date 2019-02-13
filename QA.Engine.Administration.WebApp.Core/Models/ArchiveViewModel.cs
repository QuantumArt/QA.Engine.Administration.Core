using QA.Engine.Administration.WebApp.Core.Annotations;
using System.Collections.Generic;
using System.Linq;

namespace QA.Engine.Administration.WebApp.Core.Models
{
    /// <summary>
    /// Структура архива
    /// </summary>
    [TypeScriptType]
    public class ArchiveViewModel
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
        public bool IsPage { get; set; }
        public string Discriminator { get; set; }
        public int DiscriminatorId { get; set; }
        public string DiscriminatorTitle { get; set; }
        public string IconUrl { get; set; }

        public List<ArchiveViewModel> Children { get; set; }
        public List<RegionViewModel> Regions { get; set; }

        public bool HasChildren { get { return Children?.Any() ?? false; } }
        public bool HasRegions { get { return Regions?.Any() ?? false; } }
    }
}
