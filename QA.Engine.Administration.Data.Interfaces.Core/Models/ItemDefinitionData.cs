namespace QA.Engine.Administration.Data.Interfaces.Core.Models
{
    public class ItemDefinitionData
    {
        public int Id { get; set; }
        public string Discriminator { get; set; }
        public string TypeName { get; set; }
        public bool IsPage { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string IconUrl { get; set; }
        public int? PreferredContentId { get; set; }
    }
}
