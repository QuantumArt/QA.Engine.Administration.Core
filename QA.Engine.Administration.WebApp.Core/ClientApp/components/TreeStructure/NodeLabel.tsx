import * as React from 'react';
import { inject, observer } from 'mobx-react';
import TreeStore from 'stores/TreeStore';
import { ITreeElement } from 'stores/TreeStore/BaseTreeStore';
import TreeStoreType from 'enums/TreeStoreType';

interface Props {
    treeStore?: TreeStore;
    type: TreeStoreType;
    node: ITreeElement;
}

@inject('treeStore')
@observer
export default class NodeLabel extends React.Component<Props> {

    private resolveStore = () => {
        const { treeStore, type } = this.props;
        switch (type) {
            case TreeStoreType.SITE:
            case TreeStoreType.ARCHIVE:
                return treeStore.resolveMainTreeStore();
            case TreeStoreType.WIDGET:
                return treeStore.getWidgetTreeStore();
            case TreeStoreType.CONTENTVERSION:
                return treeStore.getContentVersionTreeStore();
            case TreeStoreType.MOVE:
                return treeStore.getMoveTreeStore();
            default:
                return null;
        }
    }

    render() {
        const { node } = this.props;
        const store = this.resolveStore();
        if (!store) {
            return null;
        }
        return (
            <span className="bp3-tree-node-label">
                {store.showIDs ? node.idTitle : node.title}
            </span>
        );
    }
}
