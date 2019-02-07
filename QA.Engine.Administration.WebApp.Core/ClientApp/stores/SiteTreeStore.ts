import * as React from 'react';
import { action, observable } from 'mobx';
import siteTreeService from 'services/siteTreeService';
import { IconName, ITreeNode } from '@blueprintjs/core';
import ContextMenu from 'components/SiteTree/ContextMenu';

enum TreeState {
    NONE,
    PENDING,
    ERROR,
    SUCCESS,
}

export interface ITreeElement extends ITreeNode {
    childNodes: ITreeElement[];
    parentId: number;
    isContextMenuActive: boolean;
}

export default class SiteTreeStore {
    @observable public siteTreeState: TreeState = TreeState.NONE;
    @observable public tree: ITreeElement[] = [];

    @action
    public handleNodeExpand = (nodeData: ITreeElement) => {
        if (nodeData.childNodes.length !== 0 && nodeData.parentId !== null) {
            nodeData.icon = 'folder-open';
        }
        nodeData.isExpanded = true;
    }

    @action
    public handleNodeCollapse = (nodeData: ITreeElement) => {
        if (nodeData.childNodes.length !== 0 && nodeData.parentId !== null) {
            nodeData.icon = 'folder-close';
        }
        nodeData.isExpanded = false;
    }

    @action
    public handleNodeClick = (nodeData: ITreeElement) => {
        const originallySelected = nodeData.isSelected;
        this.forEachNode(this.tree, (n) => {
            n.isSelected = false;
            n.isContextMenuActive = false;
        });
        nodeData.isSelected = originallySelected == null ? true : !originallySelected;
    }

    @action
    public handleContextMenu = (nodeData: ITreeElement) => {
        nodeData.isContextMenuActive = !nodeData.isContextMenuActive;
    }

    @action
    public async fetchSiteTree() {
        this.siteTreeState = TreeState.PENDING;
        try {
            const res: Models.PageViewModel[] = await siteTreeService.getSiteTree();
            this.siteTreeState = TreeState.SUCCESS;
            this.convertTree(res);
        } catch (e) {
            console.log(e);
            this.siteTreeState = TreeState.ERROR;
        }
    }

    private convertTree(data: Models.PageViewModel[]): void {
        const hMap = new Map<number, ITreeElement>();
        const mapElement = (el: Models.PageViewModel): ITreeElement => {
            const hasChildren = el.children.length !== 0;
            const isRootNode = el.parentId === null;
            const getIcon = (): IconName => {
                if (isRootNode) {
                    return 'application';
                }
                if (!hasChildren) {
                    return 'document';
                }
                return 'folder-close';
            };

            const treeElement = observable<ITreeElement>({
                id: el.id,
                parentId: el.parentId,
                childNodes: [],
                label: el.title,
                isExpanded: false,
                icon: getIcon(),
                hasCaret: hasChildren,
                isContextMenuActive: false,
            });
            treeElement.secondaryLabel = React.createElement(ContextMenu, {
                node: treeElement,
            });

            return treeElement;
        };
        const mapSubtree = (elements: Models.PageViewModel[]): void => {
            elements.forEach((el: Models.PageViewModel) => {
                if (el.children.length !== 0) {
                    hMap.set(el.id, mapElement(el));
                    mapSubtree(el.children);
                } else {
                    hMap.set(el.id, mapElement(el));
                }
            });
        };
        mapSubtree(data);
        hMap.forEach((el, key, map) => el.parentId && map.get(el.parentId).childNodes.push(el));

        const tree: ITreeElement[] = [];
        hMap.forEach(el => el.parentId === null && tree.push(el));
        this.tree = tree;
    }

    private forEachNode(nodes: ITreeElement[], cb: (node: ITreeElement) => void): void {
        if (nodes === null) {
            return;
        }
        for (const node of nodes) {
            cb(node);
            this.forEachNode(node.childNodes, cb);
        }
    }
}
