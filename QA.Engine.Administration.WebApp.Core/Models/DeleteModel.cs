using QA.Engine.Administration.Common.Core;

namespace QA.Engine.Administration.WebApp.Core.Models
{
    /// <summary>
    /// Модель для окончательного удаления элемента
    /// </summary>
    [TypeScriptType]
    public class DeleteModel
    {
        /// <summary>
        /// Id элемента
        /// </summary>
        public int ItemId { get; set; }
        /// <summary>
        /// Удалить все версии
        /// </summary>
        public bool? IsDeleteAllVersions { get; set; }
    }
}
