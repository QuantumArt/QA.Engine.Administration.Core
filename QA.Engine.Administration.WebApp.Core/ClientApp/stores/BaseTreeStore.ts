import * as React from 'react';
import { action, observable, computed } from 'mobx';
import { IconName, ITreeNode } from '@blueprintjs/core';
import ContextMenu from 'components/SiteTree/ContextMenu';
import TreeState from 'enums/TreeState';
import TabsStore from './TabsStore';

export interface ITreeElement extends ITreeNode {
    childNodes: ITreeElement[];
    parentId: number;
    versionOfId?: number;
    isContextMenuActive: boolean;
    label: string;
}

interface ITree {
    id: number;
    parentId?: number;
    versionOfId?: number;
    title: string;
    children: ITree[];
    hasChildren: boolean;
}

export abstract class BaseTreeState<T extends {
    id: number;
    parentId?: null | number;
    versionOfId?: null | number;
    title: string;
    children: T[];
    hasChildren: boolean;
}> {

    constructor() {
        // this.fetchTree();
    }

    @observable public treeState: TreeState = TreeState.NONE;
    @observable public tree: ITreeElement[];

    private startLoad: boolean = true;
    public loadData() {
        if (this.startLoad) {
            this.startLoad = false;
            this.fetchTree();
        }
    }

    protected origTree: T[];

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
        TabsStore.setTabData(nodeData);
    }

    @action
    public handleContextMenu = (nodeData: ITreeElement) => {
        nodeData.isContextMenuActive = !nodeData.isContextMenuActive;
    }

    @action
    public async fetchTree(): Promise<any> {
        this.treeState = TreeState.PENDING;
        try {
            const response: ApiResult<T[]> = await this.getTree();
            if (response.isSuccess) {
                this.treeState = TreeState.SUCCESS;
                this.origTree = response.data;
                this.convertTree(response.data);
            } else {
                this.treeState = TreeState.ERROR;
                throw response.error;
            }
        } catch (e) {
            this.treeState = TreeState.ERROR;
            console.error(e);
        }
    }

    abstract async getTree(): Promise<ApiResult<T[]>>;

    public getNodeById(id: number): T {
        let elements = this.origTree;
        let loop = true;
        while (loop) {
            loop = false;
            const children: T[] = [];
            if (elements.filter(x => x.id === id)[0] != null) {
                return elements.filter(x => x.id === id)[0];
            }
            elements.filter(x => x.hasChildren).forEach((x) => {
                x.children.forEach(y => children.push(<T>y));
            });
            loop = children.length > 0;
            elements = children;
        }
        return null;
    }

    protected convertTree(data: T[]): void {
        const hMap = new Map<number, ITreeElement>();
        const mapElement = (el: T): ITreeElement => {
            const hasChildren = el.hasChildren;
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
                versionOfId: el.versionOfId,
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
        const mapSubtree = (elements: T[]): void => {
            elements.forEach((el: T) => {
                if (el.hasChildren) {
                    hMap.set(el.id, mapElement(el));
                    mapSubtree(<T[]>el.children);
                } else {
                    hMap.set(el.id, mapElement(el));
                }
            });
        };
        mapSubtree(data);
        hMap.forEach((el, key, map) => el.parentId && map.get(el.parentId) != null && map.get(el.parentId).childNodes.push(el));

        const tree: ITreeElement[] = [];
        hMap.forEach(el =>
            (el.parentId === null && el.versionOfId != null || !hMap.has(el.parentId == null ? el.versionOfId : el.parentId))
            && tree.push(el));
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
