import * as React from 'react';
import { action, computed, observable } from 'mobx';
import { IconName, ITreeNode } from '@blueprintjs/core';
import InteractiveZone from 'components/TreeStructure/InteractiveZone';
import ContextMenuType from 'enums/ContextMenuType';
import TreeStoreType from 'enums/TreeStoreType';
import NodeLabel from 'components/TreeStructure/NodeLabel';

export interface ITreeElement extends ITreeNode {
    title?: string;
    childNodes: ITreeElement[];
    id: number;
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

export interface MapEntity<T> {
    original: T;
    mapped: ITreeElement;
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
    isVisible?: boolean;
    published?: boolean;
}> {

    constructor(icons: ITreeIcons = defaultIcons) {
        this.icons = icons;
    }

    public abstract type: TreeStoreType;

    @observable public showIDs: boolean = false;
    @observable public showPath: boolean = false;

    @observable public searchActive: boolean = false;
    @observable public query: string = '';
    @observable public cordsUpdated: boolean = false;
    @observable protected expandLaunched: boolean = false;
    protected searchTimer: number;

    @observable.shallow public selectedNode: T;
    @observable public nodeCords = new Map<number, number>();
    @observable public pathMap = new Map<number, string>();
    @observable.ref protected treeInternal: ITreeElement[] = [];
    @observable.ref protected searchedTreeInternal: ITreeElement[] = [];
    protected nodesMap = new Map<number, MapEntity<T>>();
    protected searchedNodesMap = new Map<number, MapEntity<T>>();
    protected origTreeInternal: T[] = [];
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
        this.searchedTreeInternal = [];
        this.query = query;
        this.searchActive = query.length >= 2;
        clearTimeout(this.searchTimer);
        this.searchTimer = window.setTimeout(
            () => {
                if (this.searchActive) {
                    const results = new Set<T>();
                    const filterFunc = (node: T) => {
                        const query = this.query.toLowerCase();
                        if ((node.title && node.title.toLowerCase().includes(query)) ||
                            node.id.toString().includes(query) ||
                            (node.alias != null && node.alias.toLowerCase().includes(query))
                        ) {
                            const foundEl: T = {
                                ...node,
                                children: [],
                                parentId: null,
                            };
                            results.add(foundEl);
                        }
                        this.searchInternal(results, query, node);
                    };
                    this.forEachOrigNode(this.origTreeInternal, filterFunc);
                    this.origSearchedTreeInternal = Array.from(results);
                    this.convertTree(this.origSearchedTreeInternal, 'searchedTreeInternal');
                } else if (this.origSearchedTreeInternal.length > 0) {
                    this.origSearchedTreeInternal = [];
                    clearTimeout(this.searchTimer);
                }
            },
            500,
        );
    }
    protected searchInternal(results: Set<T>, query: string, node: T) {
    }

    @action
    public resetSearch = () => {
        if (this.searchActive) {
            this.query = '';
            this.searchActive = false;
            this.origSearchedTreeInternal = [];
        }
    }

    @action
    public updateCords = (id: number, value: number) => {
        this.nodeCords.set(id, value);
    }

    @action
    public setCordsUpdateStatus = (status: boolean) => {
        this.cordsUpdated = status;
    }

    @action
    public getNodeToScroll(): number {
        if (this.expandLaunched && this.cordsUpdated && !this.searchActive) {
            this.setCordsUpdateStatus(false);
            this.expandLaunched = false;
            return this.selectedNode.id;
        }
        return null;
    }

    @action
    public setSelectedNode = (node: ITreeElement) => {
        this.selectedNode = this.nodesMap.get(node.id).original;
    }

    @action
    public clear() {
        this.treeInternal = [];
        this.searchedTreeInternal = [];
        this.origTreeInternal = [];
        this.origSearchedTreeInternal = [];
        this.nodesMap.clear();
        this.searchedNodesMap.clear();
    }

    @action
    public async fetchTree(): Promise<void> {
        this.clear();
        const response: ApiResult<T[]> = await this.getTree();
        if (response.isSuccess) {
            this.origTreeInternal = response.data;
            this.convertTree(response.data, 'treeInternal');
            this.selectedNode = null;
        } else {
            throw response.error;
        }
    }

    @action
    public toggleIDs = () => {
        if (this.showPath) {
            this.showPath = false;
        }
        this.showIDs = !this.showIDs;
    }

    @action
    public togglePath = () => {
        if (this.showIDs) {
            this.showIDs = false;
        }
        this.showPath = !this.showPath;
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
        const targetNode = this.nodesMap.get(nodeData.id);
        const originallySelected = nodeData.isSelected;
        if (this.selectedNode !== null) {
            if (this.searchActive) {
                const node = this.searchedNodesMap.get(this.selectedNode.id);
                if (node) {
                    node.mapped.isSelected = false;
                }
            }
            this.nodesMap.get(this.selectedNode.id).mapped.isSelected = false;
        }
        this.selectedNode = nodeData.isSelected === true ? null : targetNode.original;
        if (targetNode.mapped.id === targetNode.original.id) {
            // means it's not zone
            nodeData.isSelected = originallySelected == null ? true : !originallySelected;
        }
    }

    @action
    public handleContextMenu = (nodeData: ITreeElement) => {
        nodeData.isContextMenuActive = !nodeData.isContextMenuActive;
    }

    @action
    public expandToNode = (node: ITreeElement) => {
        this.forEachNode(
            null,
            (n) => {
                n.isSelected = false;
                n.isContextMenuActive = false;
                n.isExpanded = false;
            },
            this.treeInternal,
        );
        let curNode = this.nodesMap.get(node.id);
        do {
            if (node.id === curNode.mapped.id && curNode.mapped.id === curNode.original.id) {
                curNode.mapped.isSelected = true;
            }
            if (curNode.mapped.childNodes.length > 0) {
                curNode.mapped.isExpanded = true;
            }
            curNode = this.nodesMap.get(curNode.mapped.parentId);
        } while (curNode != null);
        this.expandLaunched = true;
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

    protected getIcon = (el: T): IconName => {
        if (this.icons.checkPublication) {
            if (this.searchActive) {
                return el.published ? this.icons.nodePublished : this.icons.node;
            }
            if (el.parentId === null) {
                return el.published ? this.icons.rootPublished : this.icons.root;
            }
            if (!el.children || el.children.length === 0) {
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
        if (!el.children || el.children.length === 0) {
            return this.icons.leaf;
        }
        return this.icons.node;
    }

    protected mapElement(el: T): ITreeElement {
        const treeElement = observable.object<ITreeElement>(
            {
                className: el.isVisible ? '' : 'not-visible',
                id: el.id,
                parentId: el.parentId,
                versionOfId: el.versionOfId,
                childNodes: [],
                label: '',
                secondaryLabel: '',
                title: el.title,
                isExpanded: false,
                icon: this.getIcon(el),
                hasCaret: el.children && el.children.length > 0,
                isContextMenuActive: false,
                contextMenuType: this.contextMenuType,
                isVisible: el.isVisible,
                isPublished: el.published,
            },
            {
                label: observable.ref,
                secondaryLabel: observable.ref,
            },
            {
                deep: false,
            },
        );
        treeElement.secondaryLabel = React.createElement(InteractiveZone, {
            node: treeElement,
            type: this.type,
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

        this.pathMap = key === 'treeInternal' ? new Map<number, string>() : this.pathMap;

        elements.forEach((x) => {
            const el = this.mapElement(x);
            hMap.set(x.id, el);
            if (key === 'treeInternal') {
                this.nodesMap.set(x.id, { original: x, mapped: el });
                this.pathMap.set(x.id, '');
            } else {
                this.searchedNodesMap.set(x.id, { original: x, mapped: el });
            }
        });
        const tree: ITreeElement[] = Array.from(hMap.values());

        while (loop) {
            loop = false;
            const childNodes = new Map<number, ITreeElement>();
            let children: T[] = [];
            elements.forEach((x) => {
                if (x.children && x.children.length > 0) {
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
                    childNodes.set(el.id, el);
                    if (key === 'treeInternal') {
                        this.nodesMap.set(x.id, { original: x, mapped: el });
                        if (this.pathMap.has(el.parentId)) {
                            this.pathMap.set(el.id, `${this.pathMap.get(el.parentId)}/${hMap.get(el.parentId).title}`);
                        }
                    } else {
                        this.searchedNodesMap.set(x.id, { original: x, mapped: el });
                    }
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

    /**
     * @description method exclusively used to get node from raw array data
     */
    protected getNodeById(id: number): T {
        let elements = this.origTreeInternal;
        let loop = true;
        while (loop) {
            loop = false;
            const children: T[] = [];
            const node = elements.filter(x => x.id === id)[0];
            if (node != null) {
                return node;
            }
            elements.filter(x => x.children && x.children.length > 0).forEach((x) => {
                x.children.forEach(y => children.push(<T>y));
            });
            loop = children.length > 0;
            elements = children;
        }
        return null;
    }

    protected forEachNode(
        childFunc: (node: ITreeElement) => void = null,
        eachFunc: (node: ITreeElement) => void = null,
        tree?: ITreeElement[],
    ): void {
        let elements = tree ? tree : (this.searchActive ? this.searchedTree : this.tree);
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

    protected forEachOrigNode(nodes: T[], callback: (node: T) => void) {
        if (!nodes) {
            return;
        }

        for (const node of nodes) {
            callback(node);
            this.forEachOrigNode(node.children, callback);
        }
    }
}
