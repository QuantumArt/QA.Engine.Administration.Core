namespace QA.Engine.Administration.Data.Interfaces.Core.Models
{
    public class RegionData
    {
        public int Id { get; set; }
        public string Alias { get; set; }
        public int? ParentId { get; set; }
        public string Title { get; set; }
    }
}
