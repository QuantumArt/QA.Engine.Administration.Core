import * as React from "react";
import { inject, observer } from "mobx-react";
import classNames from "classnames";
import TreeStore from "stores/TreeStore";
import { ITreeElement } from "stores/TreeStore/BaseTreeStore";
import TreeStoreType from "enums/TreeStoreType";
import { Position, Tooltip } from "@blueprintjs/core";
import QpIntegrationStore from "stores/QpIntegrationStore";
import WidgetTreeStore from "stores/TreeStore/WidgetTreeStore";
import SiteTreeStore from "stores/TreeStore/SiteTreeStore";

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
            return !node.childNodes.length || tree.type === TreeStoreType.SITE ? (
                <a className={linkClasses} onClick={() => this.editHandler()}>
                    {`${node.title} - ${node.id}`}
                </a>
            ) : (
                <span className="bp3-tree-node-label">{`${node.title} - ${node.id}`}</span>
            );
        }
        if (tree.showPath && tree.searchActive) {
            return (
                <Tooltip
                    content={`${pathPrefix || ""}/${node.title}`}
                    boundary="viewport"
                    position={Position.BOTTOM}
                    modifiers={{
                        arrow: { enabled: false },
                    }}
                >
                    {!node.childNodes.length || tree.type === TreeStoreType.SITE ? (
                        <a
                            className={linkClasses}
                            onClick={() => this.editHandler()}
                        >{`${pathPrefix || ""}/${node.title}`}</a>
                    ) : (
                        <span className="bp3-tree-node-label">{`${
                            pathPrefix || ""
                        }/${node.title}`}</span>
                    )}
                </Tooltip>
            );
        }
        if (tree.showAlias) {
            const treeStore = tree as WidgetTreeStore | SiteTreeStore;
            const alias = treeStore.getAlias(node);

            const text = alias ? (
                <span className="bp3-tree-node-label">{`${node.title} - ${alias}`}</span>
            ) : (
                <span className="bp3-tree-node-label">{`${node.title}`}</span>
            );

            return !node.childNodes.length || tree.type === TreeStoreType.SITE ? (
                <a onClick={() => this.editHandler()} className={linkClasses}>
                    {text}
                </a>
            ) : (
                text
            );
        }
        return !node.childNodes.length || tree.type === TreeStoreType.SITE ? (
            <a className={linkClasses} onClick={() => this.editHandler()}>
                {node.title}
            </a>
        ) : (
            <span className="bp3-tree-node-label">{node.title}</span>
        );
    }
}
