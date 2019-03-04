import SiteMapService from 'services/SiteMapService';
import { BaseTreeState } from 'stores/TreeStore/BaseTreeStore';
import ContextMenuType from 'enums/ContextMenuType';
import { computed } from 'mobx';

export default class SiteTreeStore extends BaseTreeState<PageModel> {

    public get parentNode(): PageModel {
        return this.getNodeById(this.selectedNode.parentId);
    }

    @computed
    public get flatPages(): PageModel[] {
        const elements = this.origTree.map(x => Object.assign({}, x));
        let pages: PageModel[] = elements;
        let loop = true;
        while (loop) {
            loop = false;
            const children: PageModel[] = [];
            pages.forEach((x) => {
                x.children.forEach((y) => {
                    children.push(y);
                    elements.push(y);
                });
            });
            pages = children;
            loop = children.length > 0;
        }
        return elements.slice().sort((a, b) => a.title > b.title ? 1 : (a.title === b.title ? 0 : -1));
    }

    protected contextMenuType: ContextMenuType = ContextMenuType.SITEMAP;

    protected async getTree(): Promise<ApiResult<PageModel[]>> {
        return await SiteMapService.getSiteMapTree();
    }

    protected getSubTree(id: number): Promise<ApiResult<PageModel>> {
        return SiteMapService.getSiteMapSubTree(id);
    }
}
