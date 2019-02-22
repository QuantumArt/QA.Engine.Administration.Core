import { action, observable } from 'mobx';

export enum Pages {
    SITEMAP,
    ARCHIVE,
}

export enum TabTypes {
    NONE,
    COMMON,
    WIDGETS,
    CONTENT_VERSIONS,
}

export default class NavigationStore {
    @observable public currentPage: Pages = Pages.SITEMAP;
    @observable public currentTab: TabTypes = TabTypes.NONE;

    @action
    changePage(page: Pages) {
        this.currentPage = page;
    }

    @action
    changeTab = (id: TabTypes) => {
        this.currentTab = id;
    }

    @action
    setDefaultTab = (isSelected: boolean) => {
        if (isSelected === false) {
            this.currentTab = TabTypes.NONE;
        } else if (this.currentTab === TabTypes.NONE) {
            this.currentTab = TabTypes.COMMON;
        }
    }

    @action
    resetTab = () => {
        this.currentTab = TabTypes.NONE;
    }
}
