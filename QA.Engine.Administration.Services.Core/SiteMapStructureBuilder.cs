using QA.Engine.Administration.Services.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QA.Engine.Administration.Services.Core
{
    public class SiteMapStructureBuilder
    {
        public static List<PageModel> GetPageStructure(List<PageModel> pages, List<WidgetModel> widgets, List<DiscriminatorModel> discriminators)
        {
            var pageStructure = pages
                .Where(x => x.ParentId == null && x.VersionOfId == null)
                .ToList();
            if (!pageStructure.Any())
                pageStructure = pages.Where(x => !pages.Any(y => y.Id == (x.ParentId ?? x.VersionOfId))).ToList();

            if (discriminators != null && discriminators.Any())
                pageStructure.ForEach(x => x.Discriminator = discriminators.FirstOrDefault(y => y.Id == x.DiscriminatorId));
            pageStructure.ForEach(x => pages.Remove(x));

            var result = GetPageTree(pageStructure, pages, widgets, discriminators);

            return result;
        }

        public static List<PageModel> GetCurrentPageStructure(int topLevelId, List<PageModel> pages, List<WidgetModel> widgets, List<DiscriminatorModel> discriminators = null)
        {
            var pageStructure = pages
                .Where(x => x.Id == topLevelId)
                .ToList();

            if (discriminators != null && discriminators.Any())
                pageStructure.ForEach(x => x.Discriminator = discriminators.FirstOrDefault(y => y.Id == x.DiscriminatorId));
            pageStructure.ForEach(x => pages.Remove(x));

            var result = GetPageTree(pageStructure, pages, widgets, discriminators);

            return result;
        }

        public static List<WidgetModel> GetWidgetStructure(PageModel page, List<WidgetModel> widgets, List<DiscriminatorModel> discriminators)
        {
            var widgetStructure = widgets
                .Where(x => x.ParentId == page?.Id)
                .ToList();
            if (!widgetStructure.Any() && page == null)
                widgetStructure = widgets.Where(x => !widgets.Any(y => y.Id == x.ParentId)).ToList();

            if (discriminators != null && discriminators.Any())
                widgetStructure.ForEach(x => x.Discriminator = discriminators.FirstOrDefault(y => y.Id == x.DiscriminatorId));
            widgetStructure.ForEach(x => widgets.Remove(x));

            var result = GetWidgetTree(widgetStructure, widgets, discriminators);

            return result;
        }

        public static List<WidgetModel> GetCurrentWidgetStructure(int topLevelId, List<WidgetModel> widgets, List<DiscriminatorModel> discriminators = null)
        {
            var widgetStructure = widgets
                .Where(x => topLevelId == x.Id)
                .ToList();

            if (discriminators != null && discriminators.Any())
                widgetStructure.ForEach(x => x.Discriminator = discriminators.FirstOrDefault(y => y.Id == x.DiscriminatorId));
            widgetStructure.ForEach(x => widgets.Remove(x));

            var result = GetWidgetTree(widgetStructure, widgets, discriminators);

            return result;
        }

        #region private methods

        private static List<PageModel> GetPageTree(List<PageModel> pageStructure, List<PageModel> pages, List<WidgetModel> widgets, List<DiscriminatorModel> discriminators)
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
                        if (discriminators != null && discriminators.Any())
                            el.Children.ForEach(x => x.Discriminator = discriminators.FirstOrDefault(y => y.Id == x.DiscriminatorId));
                        el.Children.ForEach(x => pages.Remove(x));
                    }
                    else if (el.HasContentVersion)
                    {
                        makeTree = true;
                        tmp.AddRange(el.ContentVersions);
                        if (discriminators != null && discriminators.Any())
                            el.ContentVersions.ForEach(x => x.Discriminator = discriminators.FirstOrDefault(y => y.Id == x.DiscriminatorId));
                        el.ContentVersions.ForEach(x => pages.Remove(x));
                    }
                    //else
                    //    tmp.Add(el);

                    el.Widgets = GetWidgetStructure(el, widgets, discriminators);
                }
                elements = tmp;
            }

            return pageStructure;
        }

        private static List<WidgetModel> GetWidgetTree(List<WidgetModel> widgetStructure, List<WidgetModel> widgets, List<DiscriminatorModel> discriminators)
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
                        if (discriminators != null && discriminators.Any())
                            el.Children.ForEach(x => x.Discriminator = discriminators.FirstOrDefault(y => y.Id == x.DiscriminatorId));
                        el.Children.ForEach(x => widgets.Remove(x));
                    }
                    else
                        tmp.Add(el);
                }
                elements = tmp;
            }

            return widgetStructure;
        }

        #endregion
    }
}
