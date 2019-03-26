import * as React from 'react';
import { inject, observer } from 'mobx-react';
import TreeStore from 'stores/TreeStore';
import { ITreeElement } from 'stores/TreeStore/BaseTreeStore';
import TreeStoreType from 'enums/TreeStoreType';
import { Position, Tooltip } from '@blueprintjs/core';

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
        const pathPrefix = tree.pathMap.get(node.id);
        if (!tree) {
            return null;
        }
        if (tree.showIDs) {
            return <span className="bp3-tree-node-label">{`${node.title} - ${node.id}`}</span>;
        }
        if (tree.searchActive && tree.showPath) {
            return (
                <Tooltip
                    content={`${pathPrefix || ''}/${node.title}`}
                    boundary="viewport"
                    position={Position.BOTTOM}
                    modifiers={{
                        arrow: { enabled: false },
                    }}
                >
                    <span className="bp3-tree-node-label">{`${pathPrefix || ''}/${node.title}`}</span>
                </Tooltip>
            );
        }
        return <span className="bp3-tree-node-label">{node.title}</span>;
    }
}
