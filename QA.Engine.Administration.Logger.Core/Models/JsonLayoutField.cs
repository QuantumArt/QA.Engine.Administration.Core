using System.Collections.Generic;
using System.Linq;

namespace QA.Engine.Administration.Logger.Core.Models
{
    /// <summary>
    /// Контейнер вложенных полей (объект json)
    /// </summary>
    public class JsonLayoutField
    {
        public JsonLayoutField(IEnumerable<JsonField> attributes)
        {
            Attributes = attributes.ToList();
        }
        /// <summary>
        /// Список полей
        /// </summary>
        public IList<JsonField> Attributes { get; set; }
    }
}
