using QA.Engine.Administration.Services.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QA.Engine.Administration.Services.Core
{
    public class SiteMapStructureBuilder
    {
        public static List<SiteTreeModel> GetPageStructure(List<SiteTreeModel> pages, List<WidgetTreeModel> widgets, List<DiscriminatorModel> discriminators, int? topLevelId = null)
        {
            var pageStructure = pages
                .Where(x => (topLevelId != null && x.Id == topLevelId || topLevelId == null && x.ParentId == null) && x.IsPage && x.VersionOfId == null)
                .ToList();
            if (!pageStructure.Any() && topLevelId.HasValue)
                pageStructure = pages.Where(x => x.Id == topLevelId).ToList();
            if (!pageStructure.Any())
                pageStructure = pages.Where(x => !pages.Any(y => y.Id == (x.ParentId ?? x.VersionOfId))).ToList();

            if (discriminators != null && discriminators.Any())
                pageStructure.ForEach(x => x.Discriminator = discriminators.FirstOrDefault(y => y.Id == x.DiscriminatorId));
            pageStructure.ForEach(x => pages.Remove(x));

            var elements = pageStructure;
            var makeTree = true;
            while (makeTree)
            {
                makeTree = false;
                var tmp = new List<SiteTreeModel>();
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
                    else
                        tmp.Add(el);

                    el.Widgets = GetWidgetStructure(el, widgets, discriminators);
                }
                elements = tmp;
            }

            return pageStructure;
        }

        public static List<WidgetTreeModel> GetWidgetStructure(SiteTreeModel page, List<WidgetTreeModel> widgets, List<DiscriminatorModel> discriminators, int? topLevelId = null)
        {
            var widgetStructure = widgets
                .Where(x => topLevelId != null && topLevelId == x.Id || topLevelId == null && x.ParentId == page?.Id)
                .ToList();
            if (!widgetStructure.Any() && page == null)
                widgetStructure = widgets.Where(x => !widgets.Any(y => y.Id == x.ParentId)).ToList();

            if (discriminators != null && discriminators.Any())
                widgetStructure.ForEach(x => x.Discriminator = discriminators.FirstOrDefault(y => y.Id == x.DiscriminatorId));
            widgetStructure.ForEach(x => widgets.Remove(x));

            var elements = widgetStructure;
            var makeTree = true;
            while (makeTree)
            {
                makeTree = false;
                var tmp = new List<WidgetTreeModel>();
                foreach (var el in elements)
                {
                    el.Children = widgets.Where(x => !x.IsPage && x.ParentId == el.Id).ToList();

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
    }
}
