import SiteMapService from 'services/SiteMapService';
import { BaseTreeState } from 'stores/TreeStore/BaseTreeStore';
import ContextMenuType from 'enums/ContextMenuType';
import { action, observable } from 'mobx';
import TreeStoreType from 'enums/TreeStoreType';

export default class ContentVersionTreeStore extends BaseTreeState<PageModel> {

    public type = TreeStoreType.CONTENTVERSION;

    @observable public selectedSiteTreeNode: PageModel;

    @action
    public init(selectedNode: any) {
        this.resetSearch();
        this.selectedSiteTreeNode = selectedNode;
        if (selectedNode == null || selectedNode.contentVersions == null) {
            this.contentVersionTree = [];
        } else {
            const node = selectedNode as PageModel;
            this.contentVersionTree = node.contentVersions;
        }
        this.selectedNode = null;
        this.fetchTree();
    }

    protected readonly contextMenuType: ContextMenuType = ContextMenuType.CONTENTVERSION;

    protected async getTree(): Promise<ApiResult<PageModel[]>> {
        return await new Promise<ApiResult<PageModel[]>>((resolve) => {
            const value: ApiResult<PageModel[]> = {
                isSuccess: true,
                data: this.contentVersionTree,
                error: null,
            };
            resolve(value);
        });
    }

    protected getSubTree(id: number): Promise<ApiResult<PageModel>> {
        return SiteMapService.getSiteMapSubTree(id);
    }

    private contentVersionTree: PageModel[];
}
