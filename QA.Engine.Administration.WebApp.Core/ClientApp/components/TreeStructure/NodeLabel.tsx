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
import OverflowTooltip from "components/OverflowTooltip/OverflowTooltip";

interface State {
    overflowActive: boolean;
}
interface Props {
    treeStore?: TreeStore;
    qpIntegrationStore?: QpIntegrationStore;
    type: TreeStoreType;
    node: ITreeElement;
}

@inject("treeStore", "qpIntegrationStore")
@observer
export default class NodeLabel extends React.Component<Props, State> {
    editHandler() {
        const { node, qpIntegrationStore } = this.props;

        qpIntegrationStore.edit(node.id);
    }

    state = {
        overflowActive: false,
    };

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
                <OverflowTooltip title={`${node.title} - ${node.id}`}>
                    {!node.childNodes.length ||
                    tree.type === TreeStoreType.SITE ? (
                        <a
                            className={linkClasses}
                            onClick={() => this.editHandler()}
                        >
                            {`${node.title} - ${node.id}`}
                        </a>
                    ) : (
                        <span className="bp3-tree-node-label">{`${node.title} - ${node.id}`}</span>
                    )}
                </OverflowTooltip>
            );
        }
        if (tree.showPath) {
            const pathText =
                tree.type === TreeStoreType.SITE
                    ? `${pathPrefix || ""}/${node.title}` : `${pathPrefix || ""}`;

            return (
                <OverflowTooltip title={pathText}>
                    {!node.childNodes.length ||
                    tree.type === TreeStoreType.SITE ? (
                        <a
                            className={linkClasses}
                            onClick={() => this.editHandler()}
                        >
                            {pathText}
                        </a>
                    ) : (
                        <span className="bp3-tree-node-label">{pathText}</span>
                    )}
                </OverflowTooltip>
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

            return (
                <OverflowTooltip
                    title={alias ? `${node.title} - ${alias}` : `${node.title}`}
                >
                    {!node.childNodes.length ||
                    tree.type === TreeStoreType.SITE ? (
                        <a
                            onClick={() => this.editHandler()}
                            className={linkClasses}
                        >
                            {text}
                        </a>
                    ) : (
                        text
                    )}
                </OverflowTooltip>
            );
        }
        return (
            <OverflowTooltip title={`${node.title}`}>
                {!node.childNodes.length || tree.type === TreeStoreType.SITE ? (
                    <a
                        className={linkClasses}
                        onClick={() => this.editHandler()}
                    >
                        {node.title}
                    </a>
                ) : (
                    <span className="bp3-tree-node-label">{node.title}</span>
                )}
            </OverflowTooltip>
        );
    }
}
