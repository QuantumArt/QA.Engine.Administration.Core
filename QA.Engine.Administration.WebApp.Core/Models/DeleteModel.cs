using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QA.Engine.Administration.WebApp.Core.Models
{
    public class DeleteModel
    {
        public int ItemId { get; set; }
        public bool? IsDeleteAllVersions { get; set; }
    }
}
