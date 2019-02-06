import * as React from 'react';
import { action, observable } from 'mobx';
import siteTreeService, { IApiResultWidgetTreeModel, ISiteTreeModel, IWidgetTreeModel } from 'services/siteTreeService';
import { IconName, ITreeNode } from '@blueprintjs/core';
import ContextMenu from 'components/SiteTree/ContextMenu';

enum TreeState {
    NONE,
    PENDING,
    ERROR,
    SUCCESS,
}

export interface ITreeElement extends ITreeNode {
    parentId: number;
}

export default class SiteTreeStore {
    @observable public siteTreeState: TreeState = TreeState.NONE;
    @observable public tree: ITreeElement[] = [];

    @action.bound
    public handleNodeExpand = (nodeData: ITreeElement) => {
        if (nodeData.childNodes.length !== 0 && nodeData.parentId !== null) {
            nodeData.icon = 'folder-open';
        }
        nodeData.isExpanded = true;
    }

    @action.bound
    public handleNodeCollapse = (nodeData: ITreeElement) => {
        if (nodeData.childNodes.length !== 0 && nodeData.parentId !== null) {
            nodeData.icon = 'folder-close';
        }
        nodeData.isExpanded = false;
    }

    @action.bound
    public handleContextMenu = (nodeData: ISiteTreeModel) => {
        console.log(nodeData.alias);
    }

    @action
    public async fetchSiteTree() {
        this.siteTreeState = TreeState.PENDING;
        try {
            const res: IApiResultWidgetTreeModel = await siteTreeService.getSiteTree();
            this.siteTreeState = TreeState.SUCCESS;
            this.convertTree(res.data);
        } catch (e) {
            console.log(e);
            this.siteTreeState = TreeState.ERROR;
        }
    }

    private convertTree(data: ISiteTreeModel[]): void {
        const hMap = new Map<number, ITreeElement>();
        const mapElement = (el: ISiteTreeModel): ITreeElement => {
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
            return {
                id: el.id,
                parentId: el.parentId,
                childNodes: [],
                label: el.title,
                isExpanded: false,
                icon: getIcon(),
                hasCaret: hasChildren,
                secondaryLabel: React.createElement(ContextMenu, {
                    isOpen: false,
                    node: el,
                }),
            };
        };
        const mapSubtree = (elements: IWidgetTreeModel[]): void => {
            elements.forEach((el: IWidgetTreeModel) => {
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
}
