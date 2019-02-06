import SiteTreeStore from './SiteTreeStore';
import ArchiveStore from './ArchiveStore';

export default function configureStores() {

    return {
        siteTreeStore: new SiteTreeStore(),
        archiveStrore: new ArchiveStore(),
    };
}
