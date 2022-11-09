import * as React from "react";
import { inject, observer } from "mobx-react";
import classNames from "classnames";
import TreeStore from "stores/TreeStore";
import { ITreeElement } from "stores/TreeStore/BaseTreeStore";
import TreeStoreType from "enums/TreeStoreType";
import { Position, Tooltip } from "@blueprintjs/core";
import QpIntegrationStore from "stores/QpIntegrationStore";
import WidgetTreeStore from "stores/TreeStore/WidgetTreeStore";

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

        const linkClasses = classNames("bp3-tree-node-label", {
            "node-selected": node.isSelected,
        });

        if (!tree) {
            return null;
        }
        if (tree.showIDs && node.id > 0) {
            return (
                <a className={linkClasses} onClick={() => this.editHandler()}>
                    {`${node.title} - ${node.id}`}
                </a>
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
                    <a
                        className={linkClasses}
                        onClick={() => this.editHandler()}
                    >{`${pathPrefix || ""}/${node.title}`}</a>
                </Tooltip>
            );
        }
        if (tree.showAlias) {
            const witgetStore = tree as WidgetTreeStore;
            const alias = witgetStore.getAlias(node);

            const text = alias ? (
                <span className="bp3-tree-node-label">{`${node.title} - ${alias}`}</span>
            ) : (
                <span className="bp3-tree-node-label">{`${node.title}`}</span>
            );

            return (
                <a onClick={() => this.editHandler()} className={linkClasses}>
                    {text}
                </a>
            );
        }
        return (
            <a className={linkClasses} onClick={() => this.editHandler()}>
                {node.title}
            </a>
        );
    }
}
