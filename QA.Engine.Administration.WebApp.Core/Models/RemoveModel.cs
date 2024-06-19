using QA.Engine.Administration.Common.Core;

namespace QA.Engine.Administration.WebApp.Core.Models
{
    /// <summary>
    /// Модель для перемещения элемента в архив
    /// </summary>
    [TypeScriptType]
    public class RemoveModel
    {
        /// <summary>
        /// Id элемента
        /// </summary>
        public int ItemId { get; set; }
        /// <summary>
        /// Удалять все версии
        /// </summary>
        public bool? IsDeleteAllVersions { get; set; }
        /// <summary>
        /// Удалить контентную версию
        /// </summary>
        public bool? IsDeleteContentVersions { get; set; }
        /// <summary>
        /// Id контентной версии, которую не удалять, а оставить в качестве полноценной страницы.
        /// Возможно только при условии IsDeleteAllVersions == false и IsDeleteContentVersions == false
        /// </summary>
        public int? ContentVersionId { get; set; }
    }
}
