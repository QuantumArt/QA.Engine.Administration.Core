using QA.Engine.Administration.Common.Core;

namespace QA.Engine.Administration.WebApp.Core.Models
{
    /// <summary>
    /// Модель для изменения отображаемой сортировки страниц
    /// </summary>
    [TypeScriptType]
    public class ReorderModel
    {
        /// <summary>
        /// Id страницы, позицию которой хотим изменить
        /// </summary>
        public int ItemId { get; set; }
        /// <summary>
        /// Id страницы, рядом с которой хотим поместить страницу
        /// </summary>
        public int RelatedItemId { get; set; }
        /// <summary>
        /// Признак - переместить страницу ItemId перед страницей RelatedItemId
        /// </summary>
        public bool IsInsertBefore { get; set; }
    }
}
