import SiteMapService from 'services/SiteMapService';
import { BaseTreeState } from 'stores/BaseTreeStore';

export class ArchiveState extends BaseTreeState<ArchiveViewModel> {
    constructor() {
        super();
    }

    async getTree(): Promise<ApiResult<ArchiveViewModel[]>> {
        return await SiteMapService.getArchiveTree();
    }
}

const archiveStore = new ArchiveState();
export default archiveStore;
