using QA.Engine.Administration.Services.Core.Models;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace QA.Engine.Administration.Services.Core
{
    public class SiteMapStructureBuilder
    {
        public static List<PageModel> GetPageTree(List<PageModel> pages, List<WidgetModel> widgets)
        {
            var pageTree = pages
                .Where(x => x.ParentId == null && x.VersionOfId == null)
                .ToList();
            if (!pageTree.Any())
                pageTree = pages.Where(x => !pages.Any(y => y.Id == (x.ParentId ?? x.VersionOfId))).ToList();

            foreach(var x in pageTree)
                pages.Remove(x);

            var result = GetPageTreeInternal(pageTree, pages, widgets);

            return result;
        }

        public static List<PageModel> GetPageSubTree(int topLevelId, List<PageModel> pages, List<WidgetModel> widgets)
        {
            var pageTree = pages
                .Where(x => x.Id == topLevelId)
                .ToList();

            pageTree.ForEach(x => pages.Remove(x));

            var result = GetPageTreeInternal(pageTree, pages, widgets);

            return result;
        }

        public static List<WidgetModel> GetWidgetSubTree(int topLevelId, List<WidgetModel> widgets)
        {
            var widgetTree = widgets
                .Where(x => topLevelId == x.Id)
                .ToList();

            widgetTree.ForEach(x => widgets.Remove(x));

            var widgetsDict = widgets.GroupBy(k => k.ParentId).ToDictionary(k => k.Key, v => v.Select(x => x).ToList());

            var result = GetWidgetTreeInternal(widgetTree, widgetsDict);

            return result;
        }

        public static List<ArchiveModel> GetArchiveTree(List<ArchiveModel> archives)
        {
            var archivesDict = archives.GroupBy(k => k.ParentId ?? k.VersionOfId)
                .Where(x => x.Key != null)
                .ToDictionary(k => k.Key, v => v.Select(x => x).ToList());
            var archiveTree = archivesDict.Where(x => !archives.Any(y => y.Id == x.Key)).SelectMany(x => x.Value).ToList();
            archiveTree.AddRange(archives.Where(x => (x.ParentId ?? x.VersionOfId) == null));

            var result = GetArchiveTreeInternal(archiveTree, archivesDict);

            return result;
        }

        public static List<ArchiveModel> GetArchiveSubTree(int topLevelId, List<ArchiveModel> archives)
        {
            var archiveTree = archives
                .Where(x => topLevelId == x.Id)
                .ToList();
            var archivesDict = archives.GroupBy(k => k.ParentId ?? k.VersionOfId)
                .Where(x => x.Key != null)
                .ToDictionary(k => k.Key, v => v.Select(x => x).ToList());

            var result = GetArchiveTreeInternal(archiveTree, archivesDict);

            return result;
        }

        #region private methods

        private static List<PageModel> GetPageTreeInternal(List<PageModel> pageTree, List<PageModel> pages, List<WidgetModel> widgets)
        {
            var pagesDict = pages.Where(x => x.ParentId != null).GroupBy(k => k.ParentId).ToDictionary(k => k.Key, v => v.Select(x => x).ToList());
            var contentVersionsDict = pages.Where(x => x.ParentId == null && x.VersionOfId != null).GroupBy(k => k.VersionOfId).ToDictionary(k => k.Key, v => v.Select(x => x).ToList());
            var widgetsDict = widgets.Where(x => x.ParentId != null).GroupBy(k => k.ParentId).ToDictionary(k => k.Key, v => v.Select(x => x).ToList());

            var elements = pageTree;
            var makeTree = true;
            while (makeTree)
            {
                makeTree = false;
                var tmp = new List<PageModel>();

                foreach (var el in elements)
                {
                    el.Children = pagesDict.ContainsKey(el.Id) ? pagesDict[el.Id] : new List<PageModel>();
                    el.ContentVersions = contentVersionsDict.ContainsKey(el.Id) ? contentVersionsDict[el.Id] : new List<PageModel>();

                    pagesDict.Remove(el.Id);
                    contentVersionsDict.Remove(el.Id);

                    if (el.HasChildren)
                    {
                        makeTree = true;
                        tmp.AddRange(el.Children);
                    }
                    else if (el.HasContentVersion)
                    {
                        makeTree = true;
                        tmp.AddRange(el.ContentVersions);
                    }

                    var widgetTree = widgetsDict.ContainsKey(el?.Id) ? widgetsDict[el?.Id] : new List<WidgetModel>();
                    if (widgetTree.Any())
                        el.Widgets = GetWidgetTreeInternal(widgetTree, widgetsDict);
                }
                elements = tmp;
            }

            return pageTree;
        }

        private static List<WidgetModel> GetWidgetTreeInternal(List<WidgetModel> widgetTree, Dictionary<int?, List<WidgetModel>> widgets)
        {
            var elements = widgetTree;
            var makeTree = true;
            while (makeTree)
            {
                makeTree = false;
                var tmp = new List<WidgetModel>();
                foreach (var el in elements)
                {
                    el.Children = widgets.ContainsKey(el.Id) ? widgets[el.Id] : new List<WidgetModel>();
                    widgets.Remove(el.Id);

                    if (el.HasChildren)
                    {
                        makeTree = true;
                        tmp.AddRange(el.Children);
                    }
                    else
                        tmp.Add(el);
                }
                elements = tmp;
            }

            return widgetTree;
        }

        private static List<ArchiveModel> GetArchiveTreeInternal(List<ArchiveModel> archiveTree, Dictionary<int?, List<ArchiveModel>> archives)
        {
            var elements = archiveTree;
            var makeTree = true;
            while (makeTree)
            {
                makeTree = false;
                var tmp = new List<ArchiveModel>();
                foreach (var el in elements)
                {
                    el.Children = archives.ContainsKey(el.Id) ? archives[el.Id] : new List<ArchiveModel>();

                    if (el.HasChildren)
                    {
                        makeTree = true;
                        tmp.AddRange(el.Children);
                    }
                    else
                        tmp.Add(el);
                }
                elements = tmp;
            }

            return archiveTree;
        }

        #endregion
    }
}
