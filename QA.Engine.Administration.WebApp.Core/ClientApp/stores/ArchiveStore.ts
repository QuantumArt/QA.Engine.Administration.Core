import { action } from 'mobx';
import archiveService from 'services/archiveService';

export default class ArchiveStore {
    public Archive: Models.ArchiveViewModel;

    @action
    public async fetchSiteTree() {
        try {
            //this.siteTreeState = TreeState.PENDING;
            const res: Models.ArchiveViewModel = await archiveService.getArchive();
            this.Archive = res;
            console.log(res);
        } catch (e) {
            console.log(e);
            //this.siteTreeState = TreeState.ERROR;
        }
    }
}
