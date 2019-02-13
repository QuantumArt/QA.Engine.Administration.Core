using QA.Engine.Administration.Services.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QA.Engine.Administration.Services.Core
{
    public class SiteMapStructureBuilder
    {
        public static List<PageModel> GetPageStructure(List<PageModel> pages, List<WidgetModel> widgets)
        {
            var pageStructure = pages
                .Where(x => x.ParentId == null && x.VersionOfId == null)
                .ToList();
            if (!pageStructure.Any())
                pageStructure = pages.Where(x => !pages.Any(y => y.Id == (x.ParentId ?? x.VersionOfId))).ToList();

            pageStructure.ForEach(x => pages.Remove(x));

            var result = GetPageTree(pageStructure, pages, widgets);

            return result;
        }

        public static List<PageModel> GetPageStructureSubtree(int topLevelId, List<PageModel> pages, List<WidgetModel> widgets)
        {
            var pageStructure = pages
                .Where(x => x.Id == topLevelId)
                .ToList();

            pageStructure.ForEach(x => pages.Remove(x));

            var result = GetPageTree(pageStructure, pages, widgets);

            return result;
        }

        public static List<WidgetModel> GetWidgetStructure(PageModel page, List<WidgetModel> widgets)
        {
            var widgetStructure = widgets
                .Where(x => x.ParentId == page?.Id)
                .ToList();
            if (!widgetStructure.Any() && page == null)
                widgetStructure = widgets.Where(x => !widgets.Any(y => y.Id == x.ParentId)).ToList();

            widgetStructure.ForEach(x => widgets.Remove(x));

            var result = GetWidgetTree(widgetStructure, widgets);

            return result;
        }

        public static List<WidgetModel> GetWidgetStructureSubtree(int topLevelId, List<WidgetModel> widgets)
        {
            var widgetStructure = widgets
                .Where(x => topLevelId == x.Id)
                .ToList();

            widgetStructure.ForEach(x => widgets.Remove(x));

            var result = GetWidgetTree(widgetStructure, widgets);

            return result;
        }

        public static List<ArchiveModel> GetArchiveStructure(List<ArchiveModel> archives)
        {
            var archiveStructure = archives.Where(x => !archives.Any(y => y.Id == (x.ParentId ?? x.VersionOfId))).ToList();

            archiveStructure.ForEach(x => archives.Remove(x));

            var result = GetArchiveTree(archiveStructure, archives);

            return result;
        }

        #region private methods

        private static List<PageModel> GetPageTree(List<PageModel> pageStructure, List<PageModel> pages, List<WidgetModel> widgets)
        {
            var elements = pageStructure;
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

                    el.Widgets = GetWidgetStructure(el, widgets);
                }
                elements = tmp;
            }

            return pageStructure;
        }

        private static List<WidgetModel> GetWidgetTree(List<WidgetModel> widgetStructure, List<WidgetModel> widgets)
        {
            var elements = widgetStructure;
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

            return widgetStructure;
        }

        private static List<ArchiveModel> GetArchiveTree(List<ArchiveModel> archiveStructure, List<ArchiveModel> archives)
        {
            var elements = archiveStructure;
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

            return archiveStructure;
        }

        #endregion
    }
}
