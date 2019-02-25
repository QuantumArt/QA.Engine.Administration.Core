import SiteMapService from 'services/SiteMapService';
import { BaseTreeState } from 'stores/TreeStore/BaseTreeStore';
import OperationState from 'enums/OperationState';
import TreeErrors from 'enums/TreeErrors';


export default class SiteTreeStore extends BaseTreeState<PageModel> {

    protected async getTree(): Promise<ApiResult<PageModel[]>> {
        return await SiteMapService.getSiteMapTree();
    }

    protected getSubTree(id: number): Promise<ApiResult<PageModel>> {
        return SiteMapService.getSiteMapSubTree(id);
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
            this.treeErrors.push({
                type: TreeErrors.publish,
                data: itemIds,
            });
        }
    }

    async archive(model: RemoveModel): Promise<void> {
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
            });
        }
    }

    async edit(model: EditModel): Promise<void> {
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
            });
        }
    }
}
