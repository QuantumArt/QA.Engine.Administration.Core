using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QA.Engine.Administration.Data.Interfaces.Core.Models
{
    public class PageData
    {
        public int Id { get; set; }
        public int? ParentId { get; set; }
        public string Alias { get; set; }
        public int? ExtensionId { get; set; }
    }
}
