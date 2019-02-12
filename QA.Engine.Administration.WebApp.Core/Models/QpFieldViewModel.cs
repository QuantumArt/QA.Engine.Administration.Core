using QA.Engine.Administration.WebApp.Core.Annotations;

namespace QA.Engine.Administration.WebApp.Core.Models
{
    /// <summary>
    /// Модель с полями контента
    /// </summary>
    [TypeScriptType]
    public class QpFieldViewModel
    {
        /// <summary>
        /// Идентификатор поля контента
        /// </summary>
        public string FieldId { get; set; }
        /// <summary>
        /// Название контента
        /// </summary>
        public string Name { get; set; }
    }
}
