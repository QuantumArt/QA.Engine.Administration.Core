import SiteMapService from 'services/SiteMapService';
import { BaseTreeState } from 'stores/BaseTreeStore';

export class SiteTreeState extends BaseTreeState<PageViewModel> {
    constructor() {
        super();
    }

    async getTree(): Promise<ApiResult<PageViewModel[]>> {
        return await SiteMapService.getSiteMapTree();
    }
}

const siteTreeStore = new SiteTreeState();
export default siteTreeStore;
