using QA.Engine.Administration.Common.Core;

namespace QA.Engine.Administration.WebApp.Core.Models
{
    /// <summary>
    /// Восстановление из архива, возможно только  в случае наличия родительского элемента не в архиве
    /// </summary>
    [TypeScriptType]
    public class RestoreModel
    {
        /// <summary>
        /// Id элемента
        /// </summary>
        public int ItemId { get; set; }
        /// <summary>
        /// Восстанавливать все версии
        /// </summary>
        public bool? IsRestoreAllVersions { get; set; }
        /// <summary>
        /// Восстанавливать виджеты
        /// </summary>
        public bool? IsRestoreWidgets { get; set; }
        /// <summary>
        /// Восстанавливать контентные версии
        /// </summary>
        public bool? IsRestoreContentVersions { get; set; }
        /// <summary>
        /// Восстанавливать дочерние элементы
        /// </summary>
        public bool? IsRestoreChildren { get; set; }
    }
}
