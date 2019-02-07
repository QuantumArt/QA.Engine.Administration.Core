import { action } from 'mobx';
import ArchiveService from 'services/ArchiveService';
import RemoveService from 'services/RemoveService';

export default class ArchiveStore {
    public archive: ArchiveViewModel;

    @action
    public async fetchSiteTree() {
        try {
            const result: ArchiveViewModel = await ArchiveService.getArchive();
            this.archive = result;
            console.log(result);
        } catch (e) {
            console.error(e);
        }
    }

    @action
    public async fetchTest() {
        try {
            const result: boolean = await RemoveService.remove(741210);
            console.log(result);
        } catch (e) {
            console.error(e);
        }
    }
}
