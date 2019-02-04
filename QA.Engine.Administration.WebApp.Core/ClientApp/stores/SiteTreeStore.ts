import { action, observable } from 'mobx';
import siteTreeService, { IApiResultWidgetTreeModel, ISiteTreeModel, IWidgetTreeModel, IDiscriminatorModel } from 'services/siteTreeService';
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
    @observable siteTreeState: TreeState = TreeState.NONE;
    @observable siteTree: ITreeNode[] = [];

    private convertTree(data: ISiteTreeModel[]): void {
        const hMap = new Map<number, ITreeElement>();
        const mapElement = (el: ISiteTreeModel): ITreeElement => ({
            id: el.id,
            parentId: el.parentId,
            childNodes: [],
            label: el.title,
            isExpanded: true,
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
        this.siteTree = tree;
    }

    @action
    async fetchSiteTree() {
        try {
            this.siteTreeState = TreeState.PENDING;
            const res: IApiResultWidgetTreeModel = await siteTreeService.getSiteTree();
            this.convertTree(res.data);
        } catch (e) {
            console.log(e);
            this.siteTreeState = TreeState.ERROR;
        }
    }
}
