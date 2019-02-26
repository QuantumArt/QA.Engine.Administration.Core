import SiteMapService from 'services/SiteMapService';
import { BaseTreeState } from 'stores/TreeStore/BaseTreeStore';
import ContextMenuType from 'enums/ContextMenuType';
import OperationState from 'enums/OperationState';

export default class ContentVersionTreeStore extends BaseTreeState<PageModel> {

    public selectedSiteTreeNode: PageModel;

    private contentVersionTree: PageModel[];

    protected contextMenuType: ContextMenuType = ContextMenuType.CONTENTVERSION;

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

    public init(selectedNode: any) {
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

    async publish(itemIds: number[]): Promise<void> {
        this.treeState = OperationState.PENDING;
        try {
            const response: ApiResult<any> = await SiteMapService.publish(itemIds);
            if (response.isSuccess) {
                this.treeState = OperationState.SUCCESS;
            } else {
                throw response.error;
            }
        } catch (e) {
            this.treeState = OperationState.ERROR;
            console.error(e);
        }
    }
}
