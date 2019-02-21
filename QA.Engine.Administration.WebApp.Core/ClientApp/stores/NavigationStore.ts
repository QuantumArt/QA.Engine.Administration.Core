import { action, observable } from 'mobx';

export enum Pages {
    SITEMAP,
    ARCHIVE,
}

export enum TabTypes {
    NONE,
    COMMON,
    WIDGETS,
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
        this.currentTab = isSelected === true ? TabTypes.COMMON : TabTypes.NONE;
    }

    @action
    resetTab = () => {
        this.currentTab = TabTypes.NONE;
    }
}
