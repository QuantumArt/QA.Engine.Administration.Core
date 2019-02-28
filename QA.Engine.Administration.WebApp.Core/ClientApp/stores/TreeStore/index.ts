import ArchiveTreeStore from './ArchiveTreeStore';
import SiteTreeStore from './SiteTreeStore';
import NavigationStore, { Pages } from '../NavigationStore';
import ContentVersionTreeStore from './ContentVersionTreeStore';
import WidgetTreeStore from './WidgetTreeStore';

/**
 * @description Facade class to access a proper tree
 */
export default class TreeStore {

    private readonly siteTreeStore: SiteTreeStore;
    private readonly archiveStore: ArchiveTreeStore;
    private readonly contentVersionsStore: ContentVersionTreeStore;
    private readonly widgetStore: WidgetTreeStore;
    private readonly navigationStore: NavigationStore;

    constructor(navigationStore: NavigationStore) {
        this.siteTreeStore = new SiteTreeStore();
        this.archiveStore = new ArchiveTreeStore();
        this.contentVersionsStore = new ContentVersionTreeStore({ root: 'panel-stats' });
        this.widgetStore = new WidgetTreeStore({ node: 'heat-grid', leaf: 'widget-button' });

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

    public getWidgetStore(): WidgetTreeStore {
        return this.widgetStore;
    }

    public async updateSubTree(): Promise<any> {
        let current = this.resolveTreeStore();
        let selectedNode = current.selectedNode;
        await current.updateSubTree(selectedNode.id);

        current = this.resolveTreeStore();
        if (current instanceof SiteTreeStore) {
            selectedNode = current.selectedNode;
            [this.contentVersionsStore, this.widgetStore].forEach(x => x.init(selectedNode));
        }
    }
}
