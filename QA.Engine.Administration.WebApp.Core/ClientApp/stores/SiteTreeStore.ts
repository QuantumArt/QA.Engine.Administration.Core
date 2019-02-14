import SiteMapService from 'services/SiteMapService';
import { BaseTreeState } from 'stores/BaseTreeStore';
import TreeState from 'enums/TreeState';

export class SiteTreeState extends BaseTreeState<PageViewModel> {
    constructor() {
        super();
    }

    async getTree(): Promise<ApiResult<PageViewModel[]>> {
        return await SiteMapService.getSiteMapTree();
    }

    async getSubTree(id: number): Promise<ApiResult<PageViewModel>> {
        return await SiteMapService.getSiteMapSubTree(id);
    }

    async publish(itemIds: number[]): Promise<any> {
        this.treeState = TreeState.PENDING;
        try {
            const response: ApiResult<any> = await SiteMapService.publish(itemIds);
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

    async remove(model: RemoveModel): Promise<any> {
        this.treeState = TreeState.PENDING;
        try {
            const response: ApiResult<any> = await SiteMapService.remove(model);
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

const siteTreeStore = new SiteTreeState();
export default siteTreeStore;
