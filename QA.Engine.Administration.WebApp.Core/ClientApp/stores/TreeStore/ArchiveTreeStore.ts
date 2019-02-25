import SiteMapService from 'services/SiteMapService';
import { BaseTreeState } from 'stores/TreeStore/BaseTreeStore';
import OperationState from 'enums/OperationState';
import ContextMenuType from 'enums/ContextMenuType';
import TreeErrors from 'enums/TreeErrors';

export default class ArchiveTreeStore extends BaseTreeState<ArchiveModel> {

    protected contextMenuType: ContextMenuType = ContextMenuType.ARCHIVE;

    protected async getTree(): Promise<ApiResult<ArchiveModel[]>> {
        return await SiteMapService.getArchiveTree();
    }

    protected getSubTree(id: number): Promise<ApiResult<ArchiveModel>> {
        return SiteMapService.getArchiveSubTree(id);
    }

    async restore(model: RestoreModel): Promise<void> {
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
            this.treeErrors.push({
                type: TreeErrors.restore,
                data: model,
                message: e,
            });
        }
    }

    async delete(model: DeleteModel): Promise<void> {
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
            this.treeErrors.push({
                type: TreeErrors.delete,
                data: model,
                message: e,
            });
        }
    }
}
