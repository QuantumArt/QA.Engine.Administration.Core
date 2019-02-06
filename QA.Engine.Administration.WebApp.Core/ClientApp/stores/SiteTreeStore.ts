import { action, observable, computed, set, autorun, reaction } from 'mobx';
import siteTreeService, { IApiResultWidgetTreeModel, ISiteTreeModel, IWidgetTreeModel } from 'services/siteTreeService';
import { ITreeNode } from '@blueprintjs/core';

enum TreeState {
    NONE,
    PENDING,
    ERROR,
}

export interface ITreeElement extends ITreeNode{
    parentId: number;
}

export default class SiteTreeStore {
    @observable public siteTreeState: TreeState = TreeState.NONE;
    @observable public tree: ITreeNode[] = [];

    @action.bound
    public handleNodeExpand = (nodeData: ITreeNode) => {
        nodeData.isExpanded = true;
    }

    @action.bound
    public handleNodeCollapse = (nodeData: ITreeNode) => {
        nodeData.isExpanded = false;
    }

    @action
    public async fetchSiteTree() {
        try {
            this.siteTreeState = TreeState.PENDING;
            const res: IApiResultWidgetTreeModel = await siteTreeService.getSiteTree();
            this.convertTree(res.data);
        } catch (e) {
            console.log(e);
            this.siteTreeState = TreeState.ERROR;
        }
    }

    private convertTree(data: ISiteTreeModel[]): void {
        const hMap = new Map<number, ITreeElement>();
        const mapElement = (el: ISiteTreeModel): ITreeElement => ({
            id: el.id,
            parentId: el.parentId,
            childNodes: [],
            label: el.title,
            isExpanded: false,
        });
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

        const tree: ITreeNode[] = [];
        hMap.forEach((el) => {
            if (el.parentId === null) {
                delete el.parentId;
                tree.push(el);
            } else {
                delete el.parentId;
            }
        });
        this.tree = tree;
    }
}
