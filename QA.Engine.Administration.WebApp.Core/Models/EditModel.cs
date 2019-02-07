using QA.Engine.Administration.WebApp.Core.Annotations;

namespace QA.Engine.Administration.WebApp.Core.Models
{
    /// <summary>
    /// Модель для редактирования
    /// </summary>
    [TypeScriptType]
    public class EditModel
    {
        /// <summary>
        /// Id элемента
        /// </summary>
        public int ItemId { get; set; }
        /// <summary>
        /// Заголовок
        /// </summary>
        public string Title { get; set; }
    }
}
