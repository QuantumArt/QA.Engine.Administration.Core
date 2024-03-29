﻿using QA.Engine.Administration.Data.Interfaces.Core.Models;
using System.Collections.Generic;

namespace QA.Engine.Administration.Data.Interfaces.Core
{
    public interface IQpDataProvider
    {
        /// <summary>
        /// Редактировать элемент
        /// </summary>
        void Edit(int siteId, int contentId, int userId, EditData editData);
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
        void Archive(int siteId, int contentId, int userId, IEnumerable<AbstractItemData> items, AbstractItemData moveContentVersion);
        /// <summary>
        /// Восстановить элементы из архива
        /// </summary>
        void Restore(int siteId, int contentId, int userId, IEnumerable<AbstractItemData> items);
        /// <summary>
        ///  Удалить элементы
        /// </summary>
        void Delete(int siteId, int contentId, int userId, IEnumerable<AbstractItemData> items);
        
        List<QpFieldData> GetFields(int siteId, int contentId);

    }
}
