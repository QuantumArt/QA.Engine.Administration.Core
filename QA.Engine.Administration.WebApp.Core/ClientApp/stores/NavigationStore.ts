import { action, observable } from 'mobx';
import { BaseTreeState } from 'stores/BaseTreeStore';
import siteTreeStore from 'stores/SiteTreeStore';
import archiveStore from 'stores/ArchiveStore';

export enum Pages {
    SITEMAP,
    ARCHIVE,
}

export enum TabTypes {
    NONE,
    COMMON,
    WIDGETS,
}

export class NavigationState {
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

    public resolveTreeStore(): BaseTreeState<ArchiveModel> | BaseTreeState<PageModel> {
        return this.currentPage === Pages.ARCHIVE ? archiveStore as BaseTreeState<ArchiveModel> : siteTreeStore as BaseTreeState<PageModel>;
    }
}

const navigationStore = new NavigationState();
export default navigationStore;
