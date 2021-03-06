export default class ErrorsTypes {
    static readonly Tree = {
        fetch: 'Error fetching a tree',
        update: 'Error updating subtree',
        publish: 'Error publishing item',
        archive: 'Error archiving item',
        edit: 'Error editing item',
        restore: 'Error restoring item',
        delete: 'Error deleting item',
        reorder: 'Error reordering items',
        move: 'Error moving item',
    };

    static readonly Popup = {
        discriminators: 'Error fetching discriminators',
        versions: 'Error fetching content versions',
    };

    static readonly ExtensionFields = {
        fetch: 'Error fetching extension fields',
    };

    static readonly QPintegration = {
        fetchQpContentFields: 'Error fetching qp content fields.',
        fetchCustomActionCode: 'Error fetching custom action code.',
        fetchCustomActionCodeNotFound: 'Custom action code doesn\'t exist. Check the Alias field of custom action',
        fetchCultures: 'Error fetching cultures.',
        fetchRegions: 'Error fetching regions.',
        updateSiteMapSubTree: 'Error updating sitemap subtree',
    };

    static readonly Texts = {
        fetch: 'Error fetching site texts',
    };

    static readonly Regions = {
        fetch: 'Error fetching regions',
    };
}
