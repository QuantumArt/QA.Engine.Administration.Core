import * as React from 'react';
import { action, computed, observable } from 'mobx';
import { IconName, ITreeNode } from '@blueprintjs/core';
import InteractiveZone from 'components/TreeStructure/InteractiveZone';
import ContextMenuType from 'enums/ContextMenuType';
import TreeStoreType from 'enums/TreeStoreType';
import NodeLabel from 'components/TreeStructure/NodeLabel';

export interface ITreeElement extends ITreeNode {
    title?: string;
    idTitle?: string;
    childNodes: ITreeElement[];
    parentId: number;
    versionOfId?: number;
    isContextMenuActive: boolean;
    contextMenuType: ContextMenuType;
    isVisible: boolean;
    isPublished: boolean;
}

export interface ITreeIcons {
    checkPublication: boolean;
    root?: IconName;
    rootPublished?: IconName;
    node?: IconName;
    nodePublished?: IconName;
    nodeOpen?: IconName;
    nodeOpenPublished?: IconName;
    leaf?: IconName;
    leafPublished?: IconName;
}

const defaultIcons: ITreeIcons = {
    checkPublication: true,
    root: 'application',
    rootPublished: 'application',
    node: 'document',
    nodePublished: 'saved',
    nodeOpen: 'document',
    nodeOpenPublished: 'saved',
    leaf: 'document',
    leafPublished: 'saved',
};

/**
 * @description Base class for tree manipulations
 */
export abstract class BaseTreeState<T extends {
    alias: string;
    id: number;
    parentId?: null | number;
    versionOfId?: null | number;
    title: string;
    children: T[];
    hasChildren: boolean;
    isVisible?: boolean;
    published?: boolean;
}> {

    constructor(icons: ITreeIcons = defaultIcons) {
        this.icons = icons;
    }

    public abstract type: TreeStoreType;

    @observable public showIDs: boolean = false;
    @observable public searchActive: boolean = false;
    @observable public query: string = '';
    @observable public selectedNode: T;
    @observable protected treeInternal: ITreeElement[];
    @observable protected searchedTreeInternal: ITreeElement[] = [];
    protected origTreeInternal: T[];
    protected origSearchedTreeInternal: T[] = [];

    @computed
    get tree(): ITreeElement[] {
        return this.treeInternal;
    }

    @computed
    get origTree(): T[] {
        return this.origTreeInternal;
    }

    @computed
    get searchedTree(): ITreeElement[] {
        return this.searchedTreeInternal;
    }

    @action
    public search = (query: string) => {
        this.query = query.toLocaleLowerCase();
        this.searchActive = query.length >= 3;
        if (this.searchActive) {
            const f = (node: T) => {
                if (node.title.toLowerCase().includes(this.query) ||
                    node.id.toString().includes(this.query) ||
                    (node.alias !== null && node.alias.toLowerCase().includes(this.query))
                ) {
                    const foundEl: T = {
                        ...node,
                        children: [],
                        hasChildren: false,
                        parentId: null,
                    };
                    this.origSearchedTreeInternal.push(foundEl);
                }
            };
            this.forEachOrigNode(this.origTreeInternal, f);
            this.convertTree(this.origSearchedTreeInternal, 'searchedTreeInternal');
        } else if (this.origSearchedTreeInternal.length > 0) {
            this.origSearchedTreeInternal = [];
        }
    }

    @action
    public async fetchTree(): Promise<void> {
        this.treeInternal = [];
        this.origSearchedTreeInternal = [];
        const response: ApiResult<T[]> = await this.getTree();
        if (response.isSuccess) {
            this.origTreeInternal = response.data;
            this.convertTree(response.data, 'treeInternal');
        } else {
            throw response.error;
        }
    }

    @action
    public toggleIDs = () => {
        this.showIDs = !this.showIDs;
    }

    @action
    public handleNodeExpand = (nodeData: ITreeElement) => {
        if (nodeData.childNodes.length !== 0 && nodeData.parentId !== null) {
            if (this.icons.checkPublication && nodeData.isPublished) {
                nodeData.icon = this.icons.nodeOpenPublished;
            } else {
                nodeData.icon = this.icons.nodeOpen;
            }
        }
        nodeData.isExpanded = true;
    }

    @action
    public handleNodeCollapse = (nodeData: ITreeElement) => {
        if (nodeData.childNodes.length !== 0 && nodeData.parentId !== null) {
            if (this.icons.checkPublication && nodeData.isPublished) {
                nodeData.icon = this.icons.nodePublished;
            } else {
                nodeData.icon = this.icons.node;
            }
        }
        nodeData.isExpanded = false;
    }

    @action
    public handleNodeClick = (nodeData: ITreeElement) => {
        this.selectedNode = nodeData.isSelected === true ? null : this.getNodeById(+nodeData.id);
        const originallySelected = nodeData.isSelected;
        this.forEachNode(null, (n) => {
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
    public async updateSubTree(id: number): Promise<void> {
        const response: ApiResult<T> = await this.getSubTree(id);
        if (response.isSuccess) {
            const updateInternal = (tree: T[], key: 'searchedTreeInternal' | 'treeInternal') => {
                const expanded: number[] = [];
                this.forEachNode((x) => {
                    if (x.isExpanded) {
                        expanded.push(+x.id);
                    }
                });

                const node = this.getNodeById(id);
                const parentNode = this.getNodeById(node.parentId == null ? node.versionOfId : node.parentId);
                const elements = parentNode == null ? tree : parentNode.children;
                for (let i = 0; i < elements.length; i += 1) {
                    if (response.data == null && elements[i].id === id) {
                        elements.splice(i, 1);
                        this.selectedNode = null;
                        break;
                    } else if (response.data != null && elements[i].id === response.data.id) {
                        elements[i] = response.data;
                        this.selectedNode = response.data;
                    }
                }

                this.convertTree(tree, key);
                this.forEachNode(
                    (x) => {
                        if (expanded.indexOf(+x.id) > -1) {
                            x.isExpanded = true;
                        }
                    },
                    (x) => {
                        if (this.selectedNode && this.selectedNode.id === +x.id) {
                            x.isSelected = true;
                        }
                    });
            };
            updateInternal(this.origTreeInternal, 'treeInternal');
            if (this.searchActive) {
                updateInternal(this.origSearchedTreeInternal, 'searchedTreeInternal');
            }
        } else {
            throw response.error;
        }
    }

    protected abstract async getTree(): Promise<ApiResult<T[]>>;

    protected abstract async getSubTree(id: number): Promise<ApiResult<T>>;

    protected abstract contextMenuType: ContextMenuType;

    protected getNodeById(id: number): T {
        // we don't need to use searched tree here to avoid tree logic doubling
        let elements = this.origTreeInternal;
        let loop = true;
        while (loop) {
            loop = false;
            const children: T[] = [];
            const node = elements.filter(x => x.id === id)[0];
            if (node != null) {
                return node;
            }
            elements.filter(x => x.hasChildren).forEach((x) => {
                x.children.forEach(y => children.push(<T>y));
            });
            loop = children.length > 0;
            elements = children;
        }
        return null;
    }

    protected getIcon = (el: T): IconName => {
        if (this.icons.checkPublication) {
            if (this.searchActive) {
                return el.published ? this.icons.nodePublished : this.icons.node;
            }
            if (el.parentId === null) {
                return el.published ? this.icons.rootPublished : this.icons.root;
            }
            if (!el.hasChildren) {
                return el.published ? this.icons.leafPublished : this.icons.leaf;
            }
            return el.published ? this.icons.nodePublished : this.icons.node;
        }
        if (this.searchActive) {
            return this.icons.leaf;
        }
        if (el.parentId === null) {
            return this.icons.root;
        }
        if (!el.hasChildren) {
            return this.icons.leaf;
        }
        return this.icons.node;
    }

    protected mapElement(el: T): ITreeElement {
        const treeElement = observable<ITreeElement>({
            id: el.id,
            parentId: el.parentId,
            versionOfId: el.versionOfId,
            childNodes: [],
            label: '',
            title: el.title,
            idTitle: `${el.title} - ${el.id}`,
            isExpanded: false,
            icon: this.getIcon(el),
            hasCaret: el.hasChildren,
            isContextMenuActive: false,
            contextMenuType: this.contextMenuType,
            isVisible: el.isVisible,
            isPublished: el.published,
        });
        treeElement.secondaryLabel = React.createElement(InteractiveZone, {
            node: treeElement,
        });
        treeElement.label = React.createElement(NodeLabel, {
            node: treeElement,
            type: this.type,
        });

        return treeElement;
    }

    protected convertTree(data: T[], key: 'searchedTreeInternal' | 'treeInternal'): void {
        let elements = data;
        let loop = true;

        let hMap = new Map<number, ITreeElement>();
        elements.forEach(x => hMap.set(x.id, this.mapElement(x)));
        const tree: ITreeElement[] = Array.from(hMap.values());

        while (loop) {
            loop = false;
            const childNodes = new Map<number, ITreeElement>();
            let children: T[] = [];
            elements.forEach((x) => {
                if (x.hasChildren) {
                    children = children.concat(x.children);
                }
            });
            children.forEach((x) => {
                loop = true;
                const el = this.mapElement(x);
                const parentId = el.parentId == null ? el.versionOfId : el.parentId;
                const treeEl = hMap.get(parentId);
                if (treeEl != null) {
                    treeEl.childNodes.push(el);
                    childNodes.set(+el.id, el);
                }
            });
            if (childNodes.size !== children.length) {
                loop = false;
                throw 'error tree convert';
            }
            hMap = childNodes;
            elements = children;
        }
        this[key] = tree;
    }

    protected icons: ITreeIcons;

    private forEachNode(childFunc: (node: ITreeElement) => void = null, eachFunc: (node: ITreeElement) => void = null): void {
        let elements = this.searchActive ? this.searchedTree : this.tree;
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

    private forEachOrigNode(nodes: T[], callback: (node: T) => void) {
        if (!nodes) {
            return;
        }

        for (const node of nodes) {
            callback(node);
            this.forEachOrigNode(node.children, callback);
        }
    }
}
