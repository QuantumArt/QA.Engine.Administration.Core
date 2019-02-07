using QA.Engine.Administration.WebApp.Core.Annotations;

namespace QA.Engine.Administration.WebApp.Core.Models
{
    [TypeScriptType]
    public class DiscriminatorViewModel
    {
        public int Id { get; set; }
        public bool IsArchive { get; set; }
        public string Discriminator { get; set; }
        public string TypeName { get; set; }
        public bool IsPage { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string IconUrl { get; set; }
        public int? PreferredContentId { get; set; }
    }
}
