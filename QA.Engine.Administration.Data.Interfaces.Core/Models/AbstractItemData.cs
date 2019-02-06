using System.Collections.Generic;

namespace QA.Engine.Administration.Data.Interfaces.Core.Models
{
    public class AbstractItemData
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
        public string Discriminator { get; set; }
        public bool IsPage { get; set; }
        public int? VersionOfId { get; set; }
        public bool Published { get; set; }
        public bool? IsInSiteMap { get; set; }
        public int DiscriminatorId { get; set; }
        public List<RegionData> Regions { get; set; }
    }
}
