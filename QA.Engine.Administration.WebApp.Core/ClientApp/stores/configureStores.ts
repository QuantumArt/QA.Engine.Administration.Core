import SiteTreeStore from './SiteTreeStore';

export default function configureStores() {

    return {
        siteTreeStore: new SiteTreeStore(),
    };
}
