import * as React from 'react';
import { action, observable } from 'mobx';
import { IconName, ITreeNode } from '@blueprintjs/core';
import ContextMenu from 'components/SiteTree/ContextMenu';
import TreeState from 'enums/TreeState';
import SiteMapService from 'services/SiteMapService';
import TabsStore from './TabsStore';
import { element } from 'prop-types';

export interface ITreeElement extends ITreeNode {
    childNodes: ITreeElement[];
    parentId: number;
    isContextMenuActive: boolean;
    label: string;
}

export class SiteTreeState {
    constructor() {
        this.fetchSiteTree();
    }

    @observable public siteTreeState: TreeState = TreeState.NONE;
    @observable public tree: ITreeElement[];
    private origTree: PageViewModel[];

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
    public async fetchSiteTree() {
        this.siteTreeState = TreeState.PENDING;
        try {
            const response: ApiResult<PageViewModel[]> = await SiteMapService.getSiteMapTree();
            if (response.isSuccess) {
                this.siteTreeState = TreeState.SUCCESS;
                this.origTree = response.data;
                this.convertTree(response.data);
            } else {
                throw response.error;
            }
        } catch (e) {
            console.error(e);
            this.siteTreeState = TreeState.ERROR;
        }
    }

    public getNodeById(id: number): PageViewModel {
        let elements = this.origTree;
        let loop = true;
        while (loop) {
            loop = false;
            const children: PageViewModel[] = [];
            if (elements.filter(x => x.id === id)[0] != null) {
                return elements.filter(x => x.id === id)[0];
            }
            elements.filter(x => x.hasChildren).forEach((x) => {
                x.children.forEach(y => children.push(y));
            });
            loop = children.length > 0;
            elements = children;
        }
        return null;
    }

    private convertTree(data: PageViewModel[]): void {
        const hMap = new Map<number, ITreeElement>();
        const mapElement = (el: PageViewModel): ITreeElement => {
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
        const mapSubtree = (elements: PageViewModel[]): void => {
            elements.forEach((el: PageViewModel) => {
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

const siteTreeStore = new SiteTreeState();
export default siteTreeStore;
