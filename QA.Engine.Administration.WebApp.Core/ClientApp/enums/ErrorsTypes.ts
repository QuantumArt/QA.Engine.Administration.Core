export enum TreeErrors {
    fetch = 'Error fetching a tree',
    update = 'Error updating subtree',
    publish = 'Error publishing item',
    archive = 'Error archiving item',
    edit = 'Error editing item',
    restore = 'Error restoring item',
    delete = 'Error deleting item',
    reorder = 'Error reordering items',
}

export enum PopupErrors {
    discriminators = 'Error fetching discriminators',
    versions = 'Error fetching content versions',
}
