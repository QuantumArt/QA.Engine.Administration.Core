import { ArchiveState } from './ArchiveStore';
import { SiteTreeState } from './SiteTreeStore';
import { NavigationState, Pages } from './NavigationStore';

export default class TreeStore {

    private siteTreeStore: SiteTreeState;
    private archiveStore: ArchiveState;
    private navigationStore: NavigationState;

    constructor(siteTreeStore: SiteTreeState, archiveStore: ArchiveState, navigationStore: NavigationState) {
        this.siteTreeStore = siteTreeStore;
        this.archiveStore = archiveStore;
        this.navigationStore = navigationStore;
    }

    public resolveTreeStore(): ArchiveState | SiteTreeState {
        if (this.navigationStore.currentPage === Pages.ARCHIVE) {
            return this.archiveStore;
        }
        return this.siteTreeStore;
    }
}
