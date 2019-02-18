import * as React from 'react';
import { action, observable, computed } from 'mobx';
import { IconName, ITreeNode } from '@blueprintjs/core';
import ContextMenu from 'components/SiteTree/ContextMenu';
import OperationState from 'enums/OperationState';
import TabsStore from './TabsStore';

export interface ITreeElement extends ITreeNode {
    childNodes: ITreeElement[];
    parentId: number;
    versionOfId?: number;
    isContextMenuActive: boolean;
    label: string;
}

export abstract class BaseTreeState<T extends {
    id: number;
    parentId?: null | number;
    versionOfId?: null | number;
    title: string;
    children: T[];
    hasChildren: boolean;
}> {

    protected constructor() {
        this.fetchTree();
    }

    @observable public treeState: OperationState = OperationState.NONE;
    @observable public tree: ITreeElement[];

    abstract async getTree(): Promise<ApiResult<T[]>>;
    abstract async getSubTree(id: number): Promise<ApiResult<T>>;

    private selectedNodeId?: string | number;
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
        this.selectedNodeId = nodeData.id;
        const originallySelected = nodeData.isSelected;
        this.forEachNode(null, (n) => {
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
        this.treeState = OperationState.PENDING;
        try {
            const response: ApiResult<T[]> = await this.getTree();
            if (response.isSuccess) {
                this.treeState = OperationState.SUCCESS;
                this.origTree = response.data;
                this.convertTree(response.data);
            } else {
                this.treeState = OperationState.ERROR;
                throw response.error;
            }
        } catch (e) {
            this.treeState = OperationState.ERROR;
            console.error(e);
        }
    }

    @action
    public async updateSubTree(id: number): Promise<any> {
        this.treeState = OperationState.PENDING;
        try {
            await this.updateSubTreeInternal(id);
            this.treeState = OperationState.SUCCESS;
        } catch (e) {
            this.treeState = OperationState.ERROR;
            console.error(e);
        }
    }

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

    protected async updateSubTreeInternal(id: number): Promise<any> {
        const response: ApiResult<T> = await this.getSubTree(id);
        if (response.isSuccess) {
            const expanded: number[] = [];
            this.forEachNode((x) => {
                if (x.isExpanded) {
                    expanded.push(+x.id);
                }
            });
            const node = this.getNodeById(id);
            const parentNode = this.getNodeById(node.parentId);
            if (parentNode == null) {
                for (let i = 0; i < this.origTree.length; i += 1) {
                    if (this.origTree[i].id === response.data.id) {
                        this.origTree[i] = response.data;
                        break;
                    }
                }
            } else {
                for (let i = 0; i < parentNode.children.length; i += 1) {
                    if (parentNode.children[i].id === id) {
                        parentNode.children[i] = response.data;
                        break;
                    }
                }
            }
            this.convertTree(this.origTree);
            this.forEachNode(
                (x) => {
                    if (expanded.indexOf(+x.id) > -1) {
                        x.isExpanded = true;
                    }
                },
                (x) => {
                    if (this.selectedNodeId === x.id) {
                        x.isSelected = true;
                    }
                });
        } else {
            throw response.error;
        }
    }

    private convertTree(data: T[]): void {
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
        const mapSubtree = (data: T[]): void => {
            let elements = data;
            let loop = true;
            while (loop) {
                loop = false;
                const children: T[] = [];
                elements.forEach(x => hMap.set(x.id, mapElement(x)));
                elements.filter(x => x.hasChildren).forEach((x) => {
                    x.children.forEach(y => children.push(<T>y));
                });
                loop = children.length > 0;
                elements = children;
            }
        };
        mapSubtree(data);
        hMap.forEach((el, key, map) => el.parentId && map.get(el.parentId) != null && map.get(el.parentId).childNodes.push(el));

        const tree: ITreeElement[] = [];
        hMap.forEach(el =>
            (el.parentId === null && el.versionOfId != null || !hMap.has(el.parentId == null ? el.versionOfId : el.parentId))
            && tree.push(el));
        this.tree = tree;
    }

    private forEachNode(childFunc: (node: ITreeElement) => void = null, eachFunc: (node: ITreeElement) => void = null): void {
        let elements = this.tree;
        let loop = true;
        while (loop) {
            loop = false;
            const children: ITreeElement[] = [];
            elements.forEach((x) => {
                if (eachFunc != null) {
                    eachFunc(x);
                }
            });
            elements.filter(x => x.childNodes.length > 0).forEach((x) => {
                if (childFunc != null) {
                    childFunc(x);
                }
                x.childNodes.forEach(y => children.push(y));
            });
            loop = children.length > 0;
            elements = children;
        }
    }
}
