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
    render() {
        const { node, treeStore, type } = this.props;
        const tree = treeStore.resolveTree(type);
        if (!tree) {
            return null;
        }
        return (
            <span className="bp3-tree-node-label">
                {tree.showIDs ? node.idTitle : node.title}
            </span>
        );
    }
}
