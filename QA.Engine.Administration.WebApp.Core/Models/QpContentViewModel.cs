using QA.Engine.Administration.WebApp.Core.Annotations;
using System.Collections.Generic;

namespace QA.Engine.Administration.WebApp.Core.Models
{
    /// <summary>
    /// Модель QP контента с его полями
    /// </summary>
    [TypeScriptType]
    public class QpContentViewModel
    {
        /// <summary>
        /// Идентификатор контента
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Поля контента
        /// </summary>
        public List<QpFieldViewModel> Fields { get; set; }
    }
}
