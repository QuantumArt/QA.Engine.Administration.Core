import * as React from 'react';
import { action, observable, computed } from 'mobx';
import v4 from 'uuid/v4';
import { IconName, ITreeNode } from '@blueprintjs/core';
import InteractiveZone from 'components/TreeStructure/InteractiveZone';
import OperationState from 'enums/OperationState';
import ContextMenuType from 'enums/ContextMenuType';
import { TreeErrors } from 'enums/ErrorsTypes';

export interface ITreeElement extends ITreeNode {
    childNodes: ITreeElement[];
    parentId: number;
    versionOfId?: number;
    isContextMenuActive: boolean;
    contextMenuType: ContextMenuType;
    isVisible: boolean;
    isPublished: boolean;
}

export interface ITreeErrorModel extends IErrorModel<TreeErrors> {
    type: TreeErrors;
    message: string;
    data?: any;
    id: string;
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

    @observable public treeState: OperationState = OperationState.NONE;
    @observable public treeErrors: ITreeErrorModel[] = [];
    @observable public selectedNode: T;

    protected treeInternal: ITreeElement[];
    protected origTree: T[];

    @computed
    get tree(): ITreeElement[] {
        return this.treeInternal;
    }

    @action
    public async fetchTree(): Promise<void> {
        this.treeInternal = [];
        this.treeState = OperationState.PENDING;
        try {
            const response: ApiResult<T[]> = await this.getTree();
            if (response.isSuccess) {
                this.origTree = response.data;
                this.convertTree(response.data);
                this.treeState = OperationState.SUCCESS;
            } else {
                this.treeState = OperationState.ERROR;
                throw response.error;
            }
        } catch (e) {
            this.treeState = OperationState.ERROR;
            this.treeErrors.push({
                type: TreeErrors.fetch,
                message: e,
                id: v4(),
            });
        }
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
    public async updateSubTree(id: number): Promise<any> {
        this.treeState = OperationState.PENDING;
        try {
            await this.updateSubTreeInternal(id);
            this.treeState = OperationState.SUCCESS;
        } catch (e) {
            this.treeState = OperationState.ERROR;
            this.treeErrors.push({
                type: TreeErrors.update,
                data: id,
                message: e,
                id: v4(),
            });
        }
    }

    @action
    public removeError = (i: number) => {
        this.treeErrors.splice(i, 1);
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
            const parentNode = this.getNodeById(node.parentId == null ? node.versionOfId : node.parentId);
            const elements = parentNode == null ? this.origTree : parentNode.children;
            for (let i = 0; i < elements.length; i += 1) {
                if (response.data == null && elements[i].id === id) {
                    elements.splice(i, 1);
                    break;
                } else if (response.data != null && elements[i].id === response.data.id) {
                    elements[i] = response.data;
                    this.selectedNode = response.data;
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
                    if (this.selectedNode.id === +x.id) {
                        x.isSelected = true;
                    }
                });
        } else {
            throw response.error;
        }
    }

    protected abstract async getTree(): Promise<ApiResult<T[]>>;

    protected abstract async getSubTree(id: number): Promise<ApiResult<T>>;

    protected abstract contextMenuType: ContextMenuType;

    protected getNodeById(id: number): T {
        let elements = this.origTree;
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
            if (el.parentId === null) {
                return el.published ? this.icons.rootPublished : this.icons.root;
            }
            if (!el.hasChildren) {
                return el.published ? this.icons.leafPublished : this.icons.leaf;
            }
            return el.published ? this.icons.nodePublished : this.icons.node;
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
            label: el.title,
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

        return treeElement;
    }

    protected convertTree(data: T[]): void {
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
        this.treeInternal = tree;
    }

    protected icons: ITreeIcons;

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
