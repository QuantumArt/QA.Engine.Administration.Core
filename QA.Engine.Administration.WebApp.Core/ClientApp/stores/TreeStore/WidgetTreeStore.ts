import v4 from 'uuid/v4';
import SiteMapService from 'services/SiteMapService';
import { BaseTreeState } from 'stores/TreeStore/BaseTreeStore';
import ContextMenuType from 'enums/ContextMenuType';
import OperationState from 'enums/OperationState';
import { observable, action } from 'mobx';
import TreeErrors from 'enums/TreeErrors';

export default class WidgetTreeStore extends BaseTreeState<WidgetModel> {

    @observable public selectedSiteTreeNode: PageModel;

    @action
    public init(selectedNode: any) {
        this.selectedSiteTreeNode = selectedNode;
        if (selectedNode == null || selectedNode.widgets == null) {
            this.widgetTree = [];
        } else {
            const node = selectedNode as PageModel;
            this.widgetTree = node.widgets.sort((a, b) => {
                if (a.zoneName < b.zoneName) {
                    return -1;
                }
                if (a.zoneName > b.zoneName) {
                    return 1;
                }
                if (a.indexOrder < b.indexOrder) {
                    return -1;
                }
                if (a.indexOrder > b.indexOrder) {
                    return 1;
                }
                return 0;
            });
        }
        this.selectedNode = null;
        this.fetchTree();
    }

    public async publish(itemIds: number[]): Promise<void> {
        this.treeState = OperationState.PENDING;
        console.log(1);
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

    protected readonly contextMenuType: ContextMenuType = ContextMenuType.WIDGET;

    protected async getTree(): Promise<ApiResult<WidgetModel[]>> {
        return await new Promise<ApiResult<WidgetModel[]>>((resolve) => {
            const value: ApiResult<WidgetModel[]> = {
                isSuccess: true,
                data: this.widgetTree,
                error: null,
            };
            resolve(value);
        });
    }

    protected getSubTree(id: number): Promise<ApiResult<WidgetModel>> {
        return SiteMapService.getSiteMapSubTree(id);
    }

    protected getTreeNodeLabel(model: WidgetModel): string {
        if (model.zoneName == null) {
            return model.title;
        }
        return `${model.title} | ${model.zoneName}`;
    }

    private widgetTree: WidgetModel[];
}
