import { action } from 'mobx';
import archiveService from 'services/archiveService';
import removeService from 'services/removeService';

export default class ArchiveStore {
    public Archive: Models.ArchiveViewModel;

    @action
    public async fetchSiteTree() {
        try {
            const result: Models.ArchiveViewModel = await archiveService.getArchive();
            this.Archive = result;
            console.log(result);
        } catch (e) {
            console.error(e);
        }
    }

    @action
    public async fetchTest() {
        try {
            const result: boolean = await removeService.remove(741210);
            console.log(result);
        } catch (e) {
            console.error(e);
        }
    }
}
