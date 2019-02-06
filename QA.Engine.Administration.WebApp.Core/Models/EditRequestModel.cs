namespace QA.Engine.Administration.WebApp.Core.Models
{
    /// <summary>
    /// Модель для редактирования
    /// </summary>
    public class EditRequestModel
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
