using QA.Engin.Administration.Common.Core;

namespace QA.Engine.Administration.Services.Core.Models
{
    [TypeScriptType]
    public class DiscriminatorModel
    {
        public int Id { get; set; }
        public bool IsArchive { get; set; }

        public string Discriminator { get; set; }

        public string TypeName { get; set; }

        public bool IsPage { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string IconUrl { get; set; }

        public string IconClass { get; set; }

        public string IconIntent { get; set; }

        public int? PreferredContentId { get; set; }
    }
}
