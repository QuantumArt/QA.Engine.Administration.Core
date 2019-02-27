import v4 from 'uuid/v4';
import SiteMapService from 'services/SiteMapService';
import { BaseTreeState } from 'stores/TreeStore/BaseTreeStore';
import ContextMenuType from 'enums/ContextMenuType';
import OperationState from 'enums/OperationState';
import { observable, action } from 'mobx';
import TreeErrors from 'enums/TreeErrors';

export default class ContentVersionTreeStore extends BaseTreeState<PageModel> {

    @observable public selectedSiteTreeNode: PageModel;

    @action
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

    public async publish(itemIds: number[]): Promise<void> {
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
            this.treeErrors.push({
                type: TreeErrors.publish,
                data: itemIds,
                message: e,
                id: v4(),
            });
        }
    }

    public async archive(model: RemoveModel): Promise<void> {
        this.treeState = OperationState.PENDING;
        try {
            const response: ApiResult<any> = await SiteMapService.archive(model);
            if (response.isSuccess) {
                this.treeState = OperationState.SUCCESS;
            } else {
                throw response.error;
            }
        } catch (e) {
            this.treeState = OperationState.ERROR;
            this.treeErrors.push({
                type: TreeErrors.archive,
                data: model,
                message: e,
                id: v4(),
            });
        }
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
