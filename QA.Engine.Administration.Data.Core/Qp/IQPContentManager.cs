using QA.Engine.Administration.Data.Interfaces.Core.Models;
using Quantumart.QPublishing.Info;
using System.Collections.Generic;

namespace QA.Engine.Administration.Data.Core.Qp
{
    /// <summary>
    /// Контракт для работы с БД QP
    /// </summary>
    public interface IQpContentManager
    {
        #region Prepare properties

        /// <summary>
        /// Устанавливает интервал кэширования
        /// </summary>
        /// <param name="cacheInterval"></param>
        /// <returns></returns>
        IQpContentManager CacheInterval(double cacheInterval);

        /// <summary>
        /// Устанавливает подключение к QP
        /// </summary>
        /// <param name="connectionString">Строка подключения</param>
        /// <returns></returns>
        IQpContentManager Connection(string connectionString);

        /// <summary>
        /// Устанавливает название контента
        /// </summary>
        /// <param name="contentName">Имя контента</param>
        /// <returns></returns>
        IQpContentManager ContentName(string contentName);

        /// <summary>
        /// Устанавливает список полей
        /// </summary>
        /// <param name="fields">Поля через запятую</param>
        /// <returns></returns>
        IQpContentManager Fields(string fields);

        /// <summary>
        /// Устанавливает список дополнительных контентов
        /// </summary>
        /// <param name="path">Имя поля</param>
        /// <returns></returns>
        IQpContentManager Include(string path);

        /// <summary>
        /// Устанавливает признак кэширования
        /// </summary>
        /// <param name="isCacheResult">Признак кэширования</param>
        /// <returns></returns>
        IQpContentManager IsCacheResult(bool isCacheResult);

        /// <summary>
        /// Устанавливает признак получения архивных записей
        /// </summary>
        /// <param name="isIncludeArchive"></param>
        /// <returns></returns>
        IQpContentManager IsIncludeArchive(bool isIncludeArchive);

        /// <summary>
        /// Устанаваливает признак сброса кэша
        /// </summary>
        /// <param name="isResetCache"></param>
        /// <returns></returns>
        IQpContentManager IsResetCache(bool isResetCache);

        /// <summary>
        /// Устанаваливает признак показа расщепленной версии записи
        /// </summary>
        /// <param name="isShowSplittedArticle"></param>
        /// <returns></returns>
        IQpContentManager IsShowSplittedArticle(bool isShowSplittedArticle);

        /// <summary>
        /// Устанавливает использование клиентской выборки
        /// </summary>
        /// <param name="isUseClientSelection"></param>
        /// <returns></returns>
        IQpContentManager IsUseClientSelection(bool isUseClientSelection);

        /// <summary>
        /// Устанаваливает использование расписания
        /// </summary>
        /// <param name="isUseSchedule"></param>
        /// <returns></returns>
        IQpContentManager IsUseSchedule(bool isUseSchedule);

        /// <summary>
        /// Устанавливает сортировку
        /// </summary>
        /// <param name="orderBy"></param>
        /// <returns></returns>
        IQpContentManager OrderBy(string orderBy);

        /// <summary>
        /// Устанавливает размер страницы для выборки
        /// </summary>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        IQpContentManager PageSize(long pageSize);

        /// <summary>
        /// Устанавливает имя сайта
        /// </summary>
        /// <param name="siteName"></param>
        /// <returns></returns>
        IQpContentManager SiteName(string siteName);

        /// <summary>
        /// Устанавливает id контента
        /// </summary>
        /// <param name="contentId"></param>
        /// <returns></returns>
        IQpContentManager ContentId(int contentId);

        /// <summary>
        /// Устанавливает начальный индекс страницы
        /// </summary>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        IQpContentManager StartIndex(long startIndex);

        /// <summary>
        /// Устанавливает статус записей для выборки
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        IQpContentManager StatusName(QpContentItemStatus status);

        /// <summary>
        /// Устанавливает имя сайта
        /// </summary>
        /// <param name="siteName"></param>
        /// <returns></returns>
        IQpContentManager StatusName(string siteName);

        /// <summary>
        /// Устанавливает фильтр выборки
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        IQpContentManager Where(string where);

        #endregion

        /// <summary>
        /// Возвращает результат запроса
        /// </summary>
        /// <returns></returns>
        QpContentResult Get();

        /// <summary>
        /// Изменяет статус элементов
        /// </summary>
        void ChangeStatus(int statusId, int userId);

        /// <summary>
        /// Изменяет порядок элементов
        /// </summary>
        void Reorder(IEnumerable<AbstractItemData> items, string colunmName, int userId);
    }
}
