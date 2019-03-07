using QA.Engin.Administration.Common.Core;

namespace QA.Engine.Administration.Services.Core.Models
{
    [TypeScriptType]
    public class EditModel
    {
        public int ItemId { get; set; }
        public string Title { get; set; }
        public bool IsInSiteMap { get; set; }
    }
}
