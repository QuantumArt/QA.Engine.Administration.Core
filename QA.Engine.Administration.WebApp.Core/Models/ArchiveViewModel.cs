using QA.Engine.Administration.WebApp.Core.Annotations;
using System.Collections.Generic;

namespace QA.Engine.Administration.WebApp.Core.Models
{
    /// <summary>
    /// Структура архива
    /// </summary>
    [TypeScriptType]
    public class ArchiveViewModel
    {
        /// <summary>
        /// Архивные страницы
        /// </summary>
        public List<PageViewModel> Pages { get; set; }
        /// <summary>
        /// Архивные виджеты
        /// </summary>
        public List<WidgetViewModel> Widgets { get; set; }
    }
}
