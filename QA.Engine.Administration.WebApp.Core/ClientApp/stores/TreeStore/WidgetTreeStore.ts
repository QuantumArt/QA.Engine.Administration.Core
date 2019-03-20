import SiteMapService from 'services/SiteMapService';
import { BaseTreeState, ITreeElement } from 'stores/TreeStore/BaseTreeStore';
import ContextMenuType from 'enums/ContextMenuType';
import { action, observable } from 'mobx';
import TreeStoreType from 'enums/TreeStoreType';
import { IconName } from '@blueprintjs/core';

export default class WidgetTreeStore extends BaseTreeState<WidgetModel> {

    public type = TreeStoreType.WIDGET;

    @observable public selectedSiteTreeNode: PageModel;

    @action
    public handleNodeExpand = (nodeData: ITreeElement) => nodeData.isExpanded = true

    @action
    public handleNodeCollapse = (nodeData: ITreeElement) => nodeData.isExpanded = false

    @action
    public init(selectedNode: any) {
        this.resetSearch();
        this.selectedSiteTreeNode = selectedNode as PageModel;
        if (selectedNode == null || selectedNode.widgets == null) {
            this.widgetTree = [];
        } else {
            const node = selectedNode as PageModel;
            this.widgetTree = node.widgets.slice().sort((a, b) => {
                if (a.zoneName < b.zoneName) {
                    return -1;
                }
                if (a.zoneName > b.zoneName) {
                    return 1;
                }
                if (a.indexOrder < b.indexOrder) {
                    return -1;
                }
                if (a.indexOrder > b.indexOrder) {
                    return 1;
                }
                return 0;
            });
        }
        this.selectedNode = null;
        this.fetchTree();
    }

    protected readonly contextMenuType: ContextMenuType = ContextMenuType.WIDGET;

    protected async getTree(): Promise<ApiResult<WidgetModel[]>> {
        return await new Promise<ApiResult<WidgetModel[]>>((resolve) => {
            const value: ApiResult<WidgetModel[]> = {
                isSuccess: true,
                data: this.widgetTree,
                error: null,
            };
            resolve(value);
        });
    }

    protected getSubTree(id: number): Promise<ApiResult<WidgetModel>> {
        return SiteMapService.getSiteMapSubTree(id);
    }

    protected getIcon = (el: WidgetModel): IconName => {
        if (this.icons.checkPublication) {
            if (this.searchActive) {
                return el.published ? this.icons.leafPublished : this.icons.leaf;
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

    protected convertTree(data: WidgetModel[], key: 'searchedTreeInternal' | 'treeInternal'): void {
        if (this.searchActive) {
            super.convertTree(data, key);
            return;
        }
        const tree: ITreeElement[] = [];
        const zones: ITreeElement[] = [];
        let path: string[] = [];
        data.forEach((x) => {
            if (key === 'treeInternal') {
                path.push(x.zoneName);
            }
            let zoneEl = zones.find(z => z.label === x.zoneName);
            if (!zoneEl) {
                zoneEl = this.mapWidgetZoneElement(x, null, -x.id);
                // if (key === 'treeInternal') {
                //     path.push(zoneEl.title);
                // }
                tree.push(zoneEl);
                zones.push(zoneEl);
            }
            const treeItem = this.mapWidgetElement(x, zoneEl.id);
            zoneEl.childNodes.push(treeItem);
            this.pathMap.set(treeItem.id, path.join('/'));
            path = [];
            this.convertTreeInternal(x, treeItem, path, key);
        });

        this[key] = tree;
    }

    private convertTreeInternal(el: WidgetModel, parent: ITreeElement, path: string[], key: 'searchedTreeInternal' | 'treeInternal'): void {
        const zones: ITreeElement[] = [];
        el.children.forEach((x) => {
            if (key === 'treeInternal') {
                path.push(x.zoneName);
                path.push(x.title);
            }
            let zoneEl = zones.find(z => z.label === x.zoneName);
            if (!zoneEl) {
                zoneEl = this.mapWidgetZoneElement(x, parent.id, -x.id);
                // if (key === 'treeInternal') {
                //     path.push(zoneEl.title);
                // }
                zones.push(zoneEl);
                parent.childNodes.push(zoneEl);
            }
            const treeItem = this.mapWidgetElement(x, zoneEl.id);
            zoneEl.childNodes.push(treeItem);
            this.pathMap.set(treeItem.id, path.join('/'));
            this.convertTreeInternal(x, treeItem, path, key);
        });
    }

    private mapWidgetElement(el: WidgetModel, parentId?: number, id?: number): ITreeElement {
        const treeElement = super.mapElement(el);
        if (this.icons.checkPublication) {
            treeElement.icon = el.published ? this.icons.leafPublished : this.icons.leaf;
        } else {
            treeElement.icon = this.icons.leaf;
        }
        if (parentId != null) {
            treeElement.parentId = parentId;
        }
        if (id != null) {
            treeElement.id = id;
        }
        return treeElement;
    }

    private mapWidgetZoneElement(x: WidgetModel, parentId?: number, id?: number): ITreeElement {
        return observable<ITreeElement>({
            id: id || x.id,
            childNodes: [],
            label: x.zoneName,
            title: x.zoneName,
            isExpanded: false,
            icon: this.icons.node,
            hasCaret: true,
            isContextMenuActive: false,
            parentId: parentId || x.parentId,
            contextMenuType: null,
            isVisible: x.isVisible,
            isPublished: x.published,
        });
    }

    private widgetTree: WidgetModel[];
}
