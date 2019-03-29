import SiteMapService from 'services/SiteMapService';
import { BaseTreeState } from 'stores/TreeStore/BaseTreeStore';
import ContextMenuType from 'enums/ContextMenuType';
import TreeStoreType from 'enums/TreeStoreType';
import DictionaryService from 'services/DictionaryService';

export default class SiteTreeStore extends BaseTreeState<PageModel> {

    public type = TreeStoreType.SITE;

    private regionIds: number[] = [];

    public get parentNode(): PageModel {
        return this.nodesMap.get(this.selectedNode.parentId).original;
    }

    public setRegions(regionId?: number) {
        this.resetSearch();
        this.regionIds = regionId == null ? [] : [regionId];
    }

    public async getRootElement(): Promise<PageModel> {
        await this.getRootPageDiscriminator();
        let node = this.selectedNode;
        while (node.parentId != null && node.discriminator !== this.rootPageDiscriminator) {
            node = this.nodesMap.get(node.parentId).original;
        }
        return node;
    }

    protected contextMenuType: ContextMenuType = ContextMenuType.SITEMAP;

    protected async getTree(): Promise<ApiResult<PageModel[]>> {
        if (this.regionIds.length === 0) {
            return await SiteMapService.getSiteMapTree();
        }
        return await SiteMapService.getSiteMapTree(this.regionIds);
    }

    protected async getSubTree(id: number): Promise<ApiResult<PageModel>> {
        return await SiteMapService.getSiteMapSubTree(id, this.regionIds);
    }

    private rootPageDiscriminator: string = null;
    private async getRootPageDiscriminator() {
        try {
            const response: ApiResult<string> = await DictionaryService.getRootPageDiscriminator();
            if (response.isSuccess) {
                this.rootPageDiscriminator = response.data;
            } else {
                this.rootPageDiscriminator = null;
            }
        } catch (e) {
            this.rootPageDiscriminator = null;
        }
    }
}
