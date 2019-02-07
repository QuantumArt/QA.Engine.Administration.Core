import SiteTreeStore from './SiteTreeStore';
import ArchiveStore from './ArchiveStore';

export default function configureStores() {
    const siteTreeStore = new SiteTreeStore();
    const archiveStrore = new ArchiveStore();

    // initial store actions
    siteTreeStore.fetchSiteTree();
    archiveStrore.fetchSiteTree();

    return { siteTreeStore, archiveStrore };
}
