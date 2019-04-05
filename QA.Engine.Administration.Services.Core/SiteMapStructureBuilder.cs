using Microsoft.Extensions.Logging;
using QA.Engine.Administration.Services.Core.Models;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace QA.Engine.Administration.Services.Core
{
    public class SiteMapStructureBuilder
    {
        public static List<PageModel> GetPageTree(List<PageModel> pages, List<WidgetModel> widgets, ILogger _logger)
        {
            var sw = new Stopwatch();
            sw.Start();
            var pageTree = pages
                .Where(x => x.ParentId == null && x.VersionOfId == null)
                .ToList();
            if (!pageTree.Any())
                pageTree = pages.Where(x => !pages.Any(y => y.Id == (x.ParentId ?? x.VersionOfId))).ToList();

            pageTree.ForEach(x => pages.Remove(x));

            sw.Stop();
            _logger.LogTrace($"Site Map Builder prepare {sw.ElapsedMilliseconds}ms");

            var result = GetPageTreeInternal(pageTree, pages, widgets, _logger);

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

        public static List<WidgetModel> GetWidgetTree(PageModel page, List<WidgetModel> widgets)
        {
            var widgetTree = widgets
                .Where(x => x.ParentId == page?.Id)
                .ToList();
            if (!widgetTree.Any() && page == null)
                widgetTree = widgets.Where(x => !widgets.Any(y => y.Id == x.ParentId)).ToList();

            widgetTree.ForEach(x => widgets.Remove(x));

            var result = GetWidgetTreeInternal(widgetTree, widgets);

            return result;
        }

        public static List<WidgetModel> GetWidgetSubTree(int topLevelId, List<WidgetModel> widgets)
        {
            var widgetTree = widgets
                .Where(x => topLevelId == x.Id)
                .ToList();

            widgetTree.ForEach(x => widgets.Remove(x));

            var result = GetWidgetTreeInternal(widgetTree, widgets);

            return result;
        }

        public static List<ArchiveModel> GetArchiveTree(List<ArchiveModel> archives)
        {
            var archiveTree = archives
                .Where(x => !archives.Any(y => y.Id == (x.ParentId ?? x.VersionOfId)))
                .ToList();

            archiveTree.ForEach(x => archives.Remove(x));

            var result = GetArchiveTreeInternal(archiveTree, archives);

            return result;
        }

        public static List<ArchiveModel> GetArchiveSubTree(int topLevelId, List<ArchiveModel> archives)
        {
            var archiveTree = archives
                .Where(x => topLevelId == x.Id)
                .ToList();

            archiveTree.ForEach(x => archives.Remove(x));

            var result = GetArchiveTreeInternal(archiveTree, archives);

            return result;
        }

        #region private methods

        private static List<PageModel> GetPageTreeInternal(List<PageModel> pageTree, List<PageModel> pages, List<WidgetModel> widgets, ILogger _logger = null)
        {
            var sw = new Stopwatch();
            sw.Stop();

            var elements = pageTree;
            var makeTree = true;
            while (makeTree)
            {
                makeTree = false;
                var tmp = new List<PageModel>();
                foreach (var el in elements)
                {
                    el.Children = pages.Where(x => x.ParentId != null && x.ParentId == el.Id).ToList();
                    el.ContentVersions = pages.Where(x => x.ParentId == null && x.VersionOfId == el.Id).ToList();

                    if (el.HasChildren)
                    {
                        makeTree = true;
                        tmp.AddRange(el.Children);
                        el.Children.ForEach(x => pages.Remove(x));
                    }
                    else if (el.HasContentVersion)
                    {
                        makeTree = true;
                        tmp.AddRange(el.ContentVersions);
                        el.ContentVersions.ForEach(x => pages.Remove(x));
                    }

                    el.Widgets = GetWidgetTree(el, widgets);
                }
                elements = tmp;
            }

            sw.Stop();
            if (_logger != null)
                _logger.LogTrace($"Site Map Builder internal {sw.ElapsedMilliseconds}ms");

            return pageTree;
        }

        private static List<WidgetModel> GetWidgetTreeInternal(List<WidgetModel> widgetTree, List<WidgetModel> widgets)
        {
            var elements = widgetTree;
            var makeTree = true;
            while (makeTree)
            {
                makeTree = false;
                var tmp = new List<WidgetModel>();
                foreach (var el in elements)
                {
                    el.Children = widgets.Where(x => x.ParentId == el.Id).ToList();

                    if (el.HasChildren)
                    {
                        makeTree = true;
                        tmp.AddRange(el.Children);
                        el.Children.ForEach(x => widgets.Remove(x));
                    }
                    else
                        tmp.Add(el);
                }
                elements = tmp;
            }

            return widgetTree;
        }

        private static List<ArchiveModel> GetArchiveTreeInternal(List<ArchiveModel> archiveTree, List<ArchiveModel> archives)
        {
            var elements = archiveTree;
            var makeTree = true;
            while (makeTree)
            {
                makeTree = false;
                var tmp = new List<ArchiveModel>();
                foreach (var el in elements)
                {
                    el.Children = archives.Where(x => (x.ParentId ?? x.VersionOfId) == el.Id).ToList();

                    if (el.HasChildren)
                    {
                        makeTree = true;
                        tmp.AddRange(el.Children);
                        el.Children.ForEach(x => archives.Remove(x));
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
