using QA.Engine.Administration.Data.Interfaces.Core.Models;
using System.Collections.Generic;

namespace QA.Engine.Administration.Data.Interfaces.Core
{
    public interface IQpDataProvider
    {
        /// <summary>
        /// Изменить статус публикации
        /// </summary>
        void Publish(int siteId, int contentId, int userId, IEnumerable<AbstractItemData> items, int statusId);
        /// <summary>
        /// Изменить порядок сортировки
        /// </summary>
        void Reorder(int siteId, int contentId, int userId, IEnumerable<AbstractItemData> items);
        /// <summary>
        /// Изменить родителя у элемента
        /// </summary>
        void Move(int siteId, int contentId, int userId, int itemId, int newParentId);
        /// <summary>
        /// Переместить элементы в архив
        /// </summary>
        void Remove(int siteId, int contentId, int userId, IEnumerable<AbstractItemData> items);
        /// <summary>
        /// Переместить контентную версию в структуру сайте, т.е. сделать не контентной версией, а полноценной страницей
        /// </summary>
        void MoveUpContentVersion(int siteId, int contentId, int userId, AbstractItemData item);
    }
}
