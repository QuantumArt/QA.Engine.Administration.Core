import SiteTreeStore from './SiteTreeStore';
import ArchiveStore from './ArchiveStore';
import TabsStore from './TabsStore';

export const siteTreeStore = new SiteTreeStore();
export const archiveStrore = new ArchiveStore();
export const tabsStore = new TabsStore();

// initial store actions
siteTreeStore.fetchSiteTree();
archiveStrore.fetchSiteTree();
