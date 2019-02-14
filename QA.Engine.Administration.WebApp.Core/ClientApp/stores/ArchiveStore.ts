import SiteMapService from 'services/SiteMapService';
import { BaseTreeState } from 'stores/BaseTreeStore';
import TreeState from 'enums/TreeState';

export class ArchiveState extends BaseTreeState<ArchiveViewModel> {
    constructor() {
        super();
    }

    async getTree(): Promise<ApiResult<ArchiveViewModel[]>> {
        return await SiteMapService.getArchiveTree();
    }

    async getSubTree(id: number): Promise<ApiResult<ArchiveViewModel>> {
        return await SiteMapService.getArchiveSubTree(id);
    }

    async restore(model: RestoreModel): Promise<any> {
        this.treeState = TreeState.PENDING;
        try {
            const response: ApiResult<any> = await SiteMapService.restore(model);
            if (response.isSuccess) {
                this.treeState = TreeState.SUCCESS;
            } else {
                this.treeState = TreeState.ERROR;
                throw response.error;
            }
        } catch (e) {
            this.treeState = TreeState.ERROR;
            console.error(e);
        }
    }

    async delete(model: DeleteModel): Promise<any> {
        this.treeState = TreeState.PENDING;
        try {
            const response: ApiResult<any> = await SiteMapService.delete(model);
            if (response.isSuccess) {
                this.treeState = TreeState.SUCCESS;
            } else {
                this.treeState = TreeState.ERROR;
                throw response.error;
            }
        } catch (e) {
            this.treeState = TreeState.ERROR;
            console.error(e);
        }
    }
}

const archiveStore = new ArchiveState();
export default archiveStore;
