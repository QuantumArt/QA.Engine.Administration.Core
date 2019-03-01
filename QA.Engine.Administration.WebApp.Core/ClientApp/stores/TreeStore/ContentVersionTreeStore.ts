import v4 from 'uuid/v4';
import SiteMapService from 'services/SiteMapService';
import { BaseTreeState } from 'stores/TreeStore/BaseTreeStore';
import ContextMenuType from 'enums/ContextMenuType';
import OperationState from 'enums/OperationState';
import { observable, action } from 'mobx';
import { TreeErrors } from 'enums/ErrorsTypes';

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
