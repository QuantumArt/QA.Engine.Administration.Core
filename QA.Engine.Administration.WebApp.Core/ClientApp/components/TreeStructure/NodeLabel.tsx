import * as React from "react";
import { inject, observer } from "mobx-react";
import TreeStore from "stores/TreeStore";
import { ITreeElement } from "stores/TreeStore/BaseTreeStore";
import TreeStoreType from "enums/TreeStoreType";
import { Position, Tooltip } from "@blueprintjs/core";
import QpIntegrationStore from "stores/QpIntegrationStore";

interface Props {
    treeStore?: TreeStore;
    qpIntegrationStore?: QpIntegrationStore;
    type: TreeStoreType;
    node: ITreeElement;
}

@inject("treeStore", "qpIntegrationStore")
@observer
export default class NodeLabel extends React.Component<Props> {
    editHandler() {
        const { node, qpIntegrationStore } = this.props;

        qpIntegrationStore.edit(node.id);
    }

    render() {
        const { node, treeStore, type } = this.props;
        const tree = treeStore.resolveTree(type);
        const pathPrefix = tree.pathMap.get(node.id);
        if (!tree) {
            return null;
        }
        if (tree.showIDs && node.id > 0) {
            return TreeStoreType.WIDGET && !node.childNodes.length ? (
                <a
                    className="bp3-tree-node-label"
                    onClick={() => this.editHandler()}
                >
                    {" "}
                    {`${node.title} - ${node.id}`}
                </a>
            ) : (
                <span className="bp3-tree-node-label">{`${node.title} - ${node.id}`}</span>
            );
        }
        if (tree.showPath) {
            return (
                <Tooltip
                    content={`${pathPrefix || ""}/${node.title}`}
                    boundary="viewport"
                    position={Position.BOTTOM}
                    modifiers={{
                        arrow: { enabled: false },
                    }}
                >
                    <span className="bp3-tree-node-label">{`${
                        pathPrefix || ""
                    }/${node.title}`}</span>
                </Tooltip>
            );
        }
        return tree.type === TreeStoreType.WIDGET && !node.childNodes.length ? (
            <a
                className="bp3-tree-node-label"
                onClick={() => this.editHandler()}
            >
                {node.title}
            </a>
        ) : (
            <span className="bp3-tree-node-label">{node.title}</span>
        );
    }
}
