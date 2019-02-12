using System.Collections.Generic;

namespace QA.Engine.Administration.Services.Core.Models
{
    public class QpContentModel
    {
        public int Id { get; set; }
        public List<QpFieldModel> Fields { get; set; }
    }
}
