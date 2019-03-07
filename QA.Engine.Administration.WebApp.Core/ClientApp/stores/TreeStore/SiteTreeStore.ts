import SiteMapService from 'services/SiteMapService';
import { BaseTreeState } from 'stores/TreeStore/BaseTreeStore';
import ContextMenuType from 'enums/ContextMenuType';
import TreeStoreType from 'enums/TreeStoreType';

export default class SiteTreeStore extends BaseTreeState<PageModel> {

    public type = TreeStoreType.SITE;

    private regionIds: number[] = [];

    public get parentNode(): PageModel {
        return this.getNodeById(this.selectedNode.parentId);
    }

    public setRegions(regionId?: number) {
        this.regionIds = regionId == null ? [] : [regionId];
    }

    protected contextMenuType: ContextMenuType = ContextMenuType.SITEMAP;

    protected async getTree(): Promise<ApiResult<PageModel[]>> {
        if (this.regionIds.length === 0) {
            return await SiteMapService.getSiteMapTree();
        }
        return await SiteMapService.getSiteMapTree(this.regionIds);
    }

    protected getSubTree(id: number): Promise<ApiResult<PageModel>> {
        return SiteMapService.getSiteMapSubTree(id, this.regionIds);
    }
}
