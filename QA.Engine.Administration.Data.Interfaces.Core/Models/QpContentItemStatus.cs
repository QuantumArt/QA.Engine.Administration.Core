using System.ComponentModel;

namespace QA.Engine.Administration.Data.Interfaces.Core.Models
{
    /// <summary>
    /// Статус записи контента
    /// </summary>
    public enum QpContentItemStatus
    {
        /// <summary>
        /// Опубликована
        /// </summary>
        [Description("Published")]
        Published,

        /// <summary>
        /// Создана
        /// </summary>
        [Description("Created")]
        Created,

        /// <summary>
        /// Согласована
        /// </summary>
        [Description("Approved")]
        Approved,

        /// <summary>
        /// Нет статуса
        /// </summary>
        [Description("None")]
        None
    }
}
