import { action, observable } from 'mobx';
// import SiteTreeStore from './SiteTreeStore';
// import ArchiveStore from './ArchiveStore';

export enum Pages {
    SITEMAP,
    ARCHIVE,
}

export class NavigationState {
    @observable public currentPage: Pages = Pages.SITEMAP;

    @action
    changePage = (page: Pages) => {
        this.currentPage = page;
    }
}

const navigationStore = new NavigationState();
export default navigationStore;
