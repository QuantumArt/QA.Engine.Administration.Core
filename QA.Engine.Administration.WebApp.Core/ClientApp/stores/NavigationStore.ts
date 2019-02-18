import { action, observable, runInAction } from 'mobx';
import SiteTreeStore from './SiteTreeStore';
import ArchiveStore from './ArchiveStore';

export enum Pages {
    SITEMAP,
    ARCHIVE,
}

export class NavigationState {
    constructor() {
        this.changePage(Pages.SITEMAP);
    }

    @observable public currentPage: Pages = Pages.SITEMAP;

    @action
    changePage = async (page: Pages): Promise<void> => {
        if (page === Pages.SITEMAP) {
            await SiteTreeStore.loadData();
        } else if (page === Pages.ARCHIVE) {
            await ArchiveStore.loadData();
        }
        runInAction(() => this.currentPage = page);
    }
}

const navigationStore = new NavigationState();
export default navigationStore;
