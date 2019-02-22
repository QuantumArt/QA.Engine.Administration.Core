import ArchiveTreeStore from './ArchiveTreeStore';
import SiteTreeStore from './SiteTreeStore';
import NavigationStore, { Pages } from '../NavigationStore';
import ContentVersionTreeStore from './ContentVersionTreeStore';

/**
 * @description Facade class to access a proper tree
 */
export default class TreeStore {

    private readonly siteTreeStore: SiteTreeStore;
    private readonly archiveStore: ArchiveTreeStore;
    private readonly contentVersionsStore: ContentVersionTreeStore;
    private readonly navigationStore: NavigationStore;

    constructor(navigationStore: NavigationStore) {
        this.siteTreeStore = new SiteTreeStore();
        this.archiveStore = new ArchiveTreeStore();
        this.contentVersionsStore = new ContentVersionTreeStore();

        this.navigationStore = navigationStore;
    }

    public resolveTreeStore(): ArchiveTreeStore | SiteTreeStore {
        if (this.navigationStore.currentPage === Pages.ARCHIVE) {
            return this.archiveStore;
        }
        return this.siteTreeStore;
    }

    public getContentVersionsStore(): ContentVersionTreeStore {
        return this.contentVersionsStore;
    }
}