using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QA.Engine.Administration.WebApp.Core.Models
{
    public class RestoreModel
    {
        public int ItemId { get; set; }

        public bool? IsRestoreAllVersions { get; set; }

        public bool? IsRestoreWidgets { get; set; }

        public bool? IsRestoreContentVersions { get; set; }

        public bool? IsRestoreChildren { get; set; }
    }
}
