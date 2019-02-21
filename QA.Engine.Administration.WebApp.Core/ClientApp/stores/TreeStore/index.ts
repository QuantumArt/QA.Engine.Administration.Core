import ArchiveTreeStore from './ArchiveTreeStore';
import SiteTreeStore from './SiteTreeStore';
import NavigationStore, { Pages } from '../NavigationStore';

/**
 * @description Facade class to access a proper tree
 */
export default class TreeStore {

    private readonly siteTreeStore: SiteTreeStore;
    private readonly archiveStore: ArchiveTreeStore;
    private readonly navigationStore: NavigationStore;

    constructor(navigationStore: NavigationStore) {
        this.siteTreeStore = new SiteTreeStore();
        this.archiveStore = new ArchiveTreeStore();
        this.navigationStore = navigationStore;
    }

    public resolveTreeStore(): ArchiveTreeStore | SiteTreeStore {
        if (this.navigationStore.currentPage === Pages.ARCHIVE) {
            return this.archiveStore;
        }
        return this.siteTreeStore;
    }
}
