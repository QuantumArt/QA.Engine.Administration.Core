import * as React from "react";
import classnames from "classnames";
import { inject, observer, Provider } from "mobx-react";

import { CustomTreeNode } from "components/TreeStructure/CustomTreeNode";
import {
    ITreeNode,
    TreeNode,
    ITreeProps,
    TreeEventHandler,
} from "@blueprintjs/core";
import { isFunction } from "util";
import TreeStore, { TreeType } from "stores/TreeStore";
import TreeStoreType from "enums/TreeStoreType";
import RightClickMenu from "components/RightClickContextMenu";
import WidgetTreeMenu from "./InteractiveZone/WidgetTreeMenu";
import QpIntegrationStore from "stores/QpIntegrationStore";
import PopupStore from "stores/PopupStore";
import TextStore from "stores/TextStore";
import SiteTreeMenu from "./InteractiveZone/SiteTreeMenu";

const DISPLAYNAME_PREFIX = "Blueprint3";
class Classes {
    static NS = "bp3";
    public static TREE = `${Classes.NS}-tree`;
    public static TREE_NODE = `${Classes.NS}-tree-node`;
    public static TREE_ROOT = `${Classes.NS}-tree-root`;
    public static TREE_NODE_LIST = `${Classes.TREE_NODE}-list`;
}

interface Props<T> extends ITreeProps<T> {
    tree: TreeType;
    qpIntegrationStore?: QpIntegrationStore;
    popupStore?: PopupStore;
    treeStore?: TreeStore;
    textStore?: TextStore;
    updateScroll?: () => void;
}

@inject(
    "treeStore",
    "qpIntegrationStore",
    "popupStore",
    "treeStore",
    "textStore"
)
@observer
export class CustomTree<T = {}> extends React.Component<Props<T>, {}> {
    public static displayName = `${DISPLAYNAME_PREFIX}.Tree`;

    public static ofType<T>() {
        return CustomTree as new (props: Props<T>) => CustomTree<T>;
    }

    public static nodeFromPath(
        path: number[],
        treeNodes: ITreeNode[]
    ): ITreeNode {
        if (path.length === 1) {
            return treeNodes[path[0]];
        }
        return CustomTree.nodeFromPath(
            path.slice(1),
            treeNodes[path[0]].childNodes
        );
    }

    private nodeRefs: { [nodeId: string]: HTMLElement } = {};

    public componentDidUpdate(): void {
        this.props.tree.setCordsUpdateStatus(true);
    }

    public render() {
        return (
            <div className={classnames(Classes.TREE, this.props.className)}>
                {this.renderNodes(this.props.contents, [], Classes.TREE_ROOT)}
            </div>
        );
    }

    public getNodeContentElement(
        nodeId: string | number
    ): HTMLElement | undefined {
        return this.nodeRefs[nodeId];
    }

    private renderNodes(
        treeNodes: ITreeNode<T>[],
        currentPath?: number[],
        className?: string
    ): JSX.Element {
        if (treeNodes == null || treeNodes.length === 0) {
            return null;
        }

        const nodeItems = treeNodes.map((node, i) => {
            const { tree } = this.props;
            const elementPath = currentPath.concat(i);
            // tslint:disable-next-line:variable-name
            const TypedTreeNode = CustomTreeNode.ofType<T>();
            return [
                !node.childNodes.length ? (
                    <RightClickMenu
                        key={node.id}
                        content={
                            <Provider
                                qpIntegrationStore={
                                    this.props.qpIntegrationStore
                                }
                                popupStore={this.props.popupStore}
                                treeStore={this.props.treeStore}
                                textStore={this.props.textStore}
                            >
                                {tree.type === TreeStoreType.WIDGET ? (
                                    <WidgetTreeMenu itemId={+node.id} />
                                ) : tree.type === TreeStoreType.SITE ? (
                                    <SiteTreeMenu itemId={+node.id} />
                                ) : null}
                            </Provider>
                        }
                    >
                        <TypedTreeNode
                            {...node}
                            key={node.id}
                            contentRef={this.handleContentRef(+node.id)}
                            depth={elementPath.length - 1}
                            onClick={this.handleNodeClick}
                            onContextMenu={this.handleNodeContextMenu}
                            onCollapse={this.handleNodeCollapse}
                            onDoubleClick={this.handleNodeDoubleClick}
                            onExpand={this.handleNodeExpand}
                            onMouseEnter={this.handleNodeMouseEnter}
                            onMouseLeave={this.handleNodeMouseLeave}
                            path={elementPath}
                        />
                    </RightClickMenu>
                ) : (
                    <TypedTreeNode
                        {...node}
                        key={node.id}
                        contentRef={this.handleContentRef(+node.id)}
                        depth={elementPath.length - 1}
                        onClick={this.handleNodeClick}
                        onContextMenu={this.handleNodeContextMenu}
                        onCollapse={this.handleNodeCollapse}
                        onDoubleClick={this.handleNodeDoubleClick}
                        onExpand={this.handleNodeExpand}
                        onMouseEnter={this.handleNodeMouseEnter}
                        onMouseLeave={this.handleNodeMouseLeave}
                        path={elementPath}
                    />
                ),
                node.isExpanded ? (
                    <React.Fragment key={`${node.id}-r`}>
                        {this.renderNodes(node.childNodes, elementPath)}
                    </React.Fragment>
                ) : null,
            ];
        });

        return (
            <ul className={classnames(Classes.TREE_NODE_LIST, className)}>
                {nodeItems}
            </ul>
        );
    }

    private handleNodeCollapse = (
        node: TreeNode<T>,
        e: React.MouseEvent<HTMLElement>
    ) => {
        this.props.updateScroll()
        this.handlerHelper(this.props.onNodeCollapse, node, e);
    };

    private handleNodeClick = (
        node: TreeNode<T>,
        e: React.MouseEvent<HTMLElement>
    ) => {
        this.handlerHelper(this.props.onNodeClick, node, e);
    };

    private handleContentRef =
        (id: number) => (node: TreeNode<T>, element: HTMLElement | null) => {
            if (element != null) {
                const { tree } = this.props;
                if (!tree.searchActive) {
                    tree.updateCords(id, element.offsetTop);
                }
                this.nodeRefs[node.props.id] = element;
            } else {
                // don't want our object to get bloated with old keys
                delete this.nodeRefs[node.props.id];
            }
        };

    private handleNodeContextMenu = (
        node: TreeNode<T>,
        e: React.MouseEvent<HTMLElement>
    ) => {
        this.handlerHelper(this.props.onNodeContextMenu, node, e);
    };

    private handleNodeDoubleClick = (
        node: TreeNode<T>,
        e: React.MouseEvent<HTMLElement>
    ) => {
        this.handlerHelper(this.props.onNodeDoubleClick, node, e);
    };

    private handleNodeExpand = (
        node: TreeNode<T>,
        e: React.MouseEvent<HTMLElement>
    ) => {
        this.props.updateScroll()
        this.handlerHelper(this.props.onNodeExpand, node, e);
    };

    private handleNodeMouseEnter = (
        node: TreeNode<T>,
        e: React.MouseEvent<HTMLElement>
    ) => {
        this.handlerHelper(this.props.onNodeMouseEnter, node, e);
    };

    private handleNodeMouseLeave = (
        node: TreeNode<T>,
        e: React.MouseEvent<HTMLElement>
    ) => {
        this.handlerHelper(this.props.onNodeMouseLeave, node, e);
    };

    private handlerHelper(
        handlerFromProps: TreeEventHandler,
        node: TreeNode<T>,
        e: React.MouseEvent<HTMLElement>
    ) {
        if (isFunction(handlerFromProps)) {
            const nodeData = CustomTree.nodeFromPath(
                node.props.path,
                this.props.contents
            );
            handlerFromProps(nodeData, node.props.path, e);
        }
    }
}
