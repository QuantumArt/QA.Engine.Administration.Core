import { action } from 'mobx';
import SiteMapService from 'services/SiteMapService';

export class ArchiveState {
    constructor() {
        // this.fetchSiteTree();
    }

    public archive: ArchiveViewModel;

    @action
    public async fetchSiteTree() {
        try {
            const response: ApiResult<ArchiveViewModel> = await SiteMapService.getArchiveTree();
            if (response.isSuccess) {
                this.archive = response.data;
                console.log(response);
            } else {
                throw response.error;
            }
        } catch (e) {
            console.error(e);
        }
    }
}

const archiveStore = new ArchiveState();
export default archiveStore;
