using QA.Engine.Administration.Common.Core;

namespace QA.Engine.Administration.Services.Core.Models
{
    [TypeScriptType]
    public class CultureModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Name { get; set; }
    }
}