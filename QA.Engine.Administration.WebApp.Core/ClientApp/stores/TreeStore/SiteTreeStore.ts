import v4 from 'uuid/v4';
import SiteMapService from 'services/SiteMapService';
import { BaseTreeState } from 'stores/TreeStore/BaseTreeStore';
import OperationState from 'enums/OperationState';
import ContextMenuType from 'enums/ContextMenuType';
import TreeErrors from 'enums/TreeErrors';
import { computed } from 'mobx';

export default class SiteTreeStore extends BaseTreeState<PageModel> {

    public async publish(itemIds: number[]): Promise<void> {
        this.treeState = OperationState.PENDING;
        try {
            const response: ApiResult<any> = await SiteMapService.publish(itemIds);
            if (response.isSuccess) {
                await Promise.all(itemIds.map(itemId => this.updateSubTreeInternal(itemId)));
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
                await this.updateSubTreeInternal(model.itemId);
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

    public async edit(model: EditModel): Promise<void> {
        this.treeState = OperationState.PENDING;
        try {
            const response: ApiResult<any> = await SiteMapService.edit(model);
            if (response.isSuccess) {
                await this.updateSubTreeInternal(model.itemId);
                this.treeState = OperationState.SUCCESS;
            } else {
                throw response.error;
            }
        } catch (e) {
            this.treeState = OperationState.ERROR;
            this.treeErrors.push({
                type: TreeErrors.edit,
                data: model,
                message: e,
                id: v4(),
            });
        }
    }

    public async reorder(model: ReorderModel): Promise<void> {
        this.treeState = OperationState.PENDING;
        try {
            const response: ApiResult<any> = await SiteMapService.reorder(model);
            if (response.isSuccess) {
                const id = this.selectedNode.parentId;
                await this.updateSubTreeInternal(id);
                this.treeState = OperationState.SUCCESS;
            } else {
                throw response.error;
            }
        } catch (e) {
            this.treeState = OperationState.ERROR;
            this.treeErrors.push({
                type: TreeErrors.reorder,
                data: model,
                message: e,
                id: v4(),
            });
        }
    }

    public async move(model: MoveModel): Promise<void> {
        this.treeState = OperationState.PENDING;
        try {
            const response: ApiResult<any> = await SiteMapService.move(model);
            if (response.isSuccess) {
                const id = this.selectedNode.parentId;
                await this.updateSubTreeInternal(id);
                await this.updateSubTreeInternal(model.newParentId);
                this.treeState = OperationState.SUCCESS;
            } else {
                throw response.error;
            }
        } catch (e) {
            this.treeState = OperationState.ERROR;
            this.treeErrors.push({
                type: TreeErrors.reorder,
                data: model,
                message: e,
                id: v4(),
            });
        }
    }

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
        return elements.sort((a, b) => a.title > b.title ? 1 : (a.title === b.title ? 0 : -1));
    }

    protected contextMenuType: ContextMenuType = ContextMenuType.SITEMAP;

    protected async getTree(): Promise<ApiResult<PageModel[]>> {
        return await SiteMapService.getSiteMapTree();
    }

    protected getSubTree(id: number): Promise<ApiResult<PageModel>> {
        return SiteMapService.getSiteMapSubTree(id);
    }
}
