using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QA.Engine.Administration.WebApp.Core.Models
{
    public class ReorderModel
    {
        public int ItemId { get; set; }

        public int RelatedItemId { get; set; }

        public bool IsInsertBefore { get; set; }
    }
}
