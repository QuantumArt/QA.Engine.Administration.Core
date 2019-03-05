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
        fetchQPAbstractItemFields: 'Error fetching abstract items fields.',
        updateSiteMapSubTree: 'Error updating sitemap subtree',
    };

    static readonly Texts = {
        fetch: 'Error fetching site texts',
    };

    static readonly Regions = {
        fetch: 'Error fetching regions',
    };
}
