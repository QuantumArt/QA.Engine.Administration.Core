using QA.Engine.Administration.Common.Core;

namespace QA.Engine.Administration.WebApp.Core.Models
{
    /// <summary>
    /// Модель для изменения родительского элемента
    /// </summary>
    [TypeScriptType]
    public class MoveModel
    {
        /// <summary>
        /// Id страницы, родительский элемент у которой хотим изменить
        /// </summary>
        public int ItemId { get; set; }
        /// <summary>
        /// Id нового родительского элемента
        /// </summary>
        public int NewParentId { get; set; }
    }
}
