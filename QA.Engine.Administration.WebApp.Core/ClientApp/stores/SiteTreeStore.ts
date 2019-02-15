import SiteMapService from 'services/SiteMapService';
import { BaseTreeState } from 'stores/BaseTreeStore';
import OperationState from 'enums/OperationState';

export class SiteTreeState extends BaseTreeState<PageModel> {
    constructor() {
        super();
    }

    async getTree(): Promise<ApiResult<PageModel[]>> {
        return await SiteMapService.getSiteMapTree();
    }

    async getSubTree(id: number): Promise<ApiResult<PageModel>> {
        return await SiteMapService.getSiteMapSubTree(id);
    }

    async publish(itemIds: number[]): Promise<any> {
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

    async remove(model: RemoveModel): Promise<any> {
        this.treeState = OperationState.PENDING;
        try {
            const response: ApiResult<any> = await SiteMapService.remove(model);
            if (response.isSuccess) {
                await this.updateSubTreeInternal(model.itemId);
                this.treeState = OperationState.SUCCESS;
            } else {
                throw response.error;
            }
        } catch (e) {
            this.treeState = OperationState.ERROR;
            console.error(e);
        }
    }

    async edit(model: EditModel): Promise<any> {
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
            console.error(e);
        }
    }
}

const siteTreeStore = new SiteTreeState();
export default siteTreeStore;
