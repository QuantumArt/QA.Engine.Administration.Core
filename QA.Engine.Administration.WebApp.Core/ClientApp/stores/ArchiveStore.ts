import { action } from 'mobx';
import SiteMapService from 'services/SiteMapService';

export default class ArchiveStore {
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

    @action
    public async fetchTest() {
        try {
            const model = <RemoveModel>{
                itemId: 741210,
                isDeleteAllVersions: true,
                isDeleteContentVersions: true,
                contentVersionId: null,
            };
            const result: boolean = await SiteMapService.remove(model);
            console.log(result);
        } catch (e) {
            console.error(e);
        }
    }
}
