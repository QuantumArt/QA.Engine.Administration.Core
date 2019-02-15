import SiteMapService from 'services/SiteMapService';
import { BaseTreeState } from 'stores/BaseTreeStore';
import OperationState from 'enums/OperationState';

export class ArchiveState extends BaseTreeState<ArchiveModel> {
    constructor() {
        super();
    }

    async getTree(): Promise<ApiResult<ArchiveModel[]>> {
        return await SiteMapService.getArchiveTree();
    }

    async getSubTree(id: number): Promise<ApiResult<ArchiveModel>> {
        return await SiteMapService.getArchiveSubTree(id);
    }

    async restore(model: RestoreModel): Promise<any> {
        this.treeState = OperationState.PENDING;
        try {
            const response: ApiResult<any> = await SiteMapService.restore(model);
            if (response.isSuccess) {
                await this.updateSubTreeInternal(model.itemId);
                this.treeState = OperationState.SUCCESS;
            } else {
                this.treeState = OperationState.ERROR;
                throw response.error;
            }
        } catch (e) {
            this.treeState = OperationState.ERROR;
            console.error(e);
        }
    }

    async delete(model: DeleteModel): Promise<any> {
        this.treeState = OperationState.PENDING;
        try {
            const response: ApiResult<any> = await SiteMapService.delete(model);
            if (response.isSuccess) {
                await this.updateSubTreeInternal(model.itemId);
                this.treeState = OperationState.SUCCESS;
            } else {
                this.treeState = OperationState.ERROR;
                throw response.error;
            }
        } catch (e) {
            this.treeState = OperationState.ERROR;
            console.error(e);
        }
    }
}

const archiveStore = new ArchiveState();
export default archiveStore;
