using System.Collections.Generic;

namespace QA.Engine.Administration.Services.Core.Models
{
    public class ArchiveTreeModel
    {
        public List<SiteTreeModel> Pages { get; set; }
        public List<WidgetTreeModel> Widgets { get; set; }
    }
}
