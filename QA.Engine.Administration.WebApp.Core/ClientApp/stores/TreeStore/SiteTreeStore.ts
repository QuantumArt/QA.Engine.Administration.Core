import SiteMapService from 'services/SiteMapService';
import { BaseTreeState } from 'stores/TreeStore/BaseTreeStore';
import ContextMenuType from 'enums/ContextMenuType';
import { computed } from 'mobx';

export default class SiteTreeStore extends BaseTreeState<PageModel> {

    public get parentNode(): PageModel {
        return this.getNodeById(this.selectedNode.parentId);
    }

    protected contextMenuType: ContextMenuType = ContextMenuType.SITEMAP;

    protected async getTree(): Promise<ApiResult<PageModel[]>> {
        return await SiteMapService.getSiteMapTree();
    }

    protected getSubTree(id: number): Promise<ApiResult<PageModel>> {
        return SiteMapService.getSiteMapSubTree(id);
    }
}
