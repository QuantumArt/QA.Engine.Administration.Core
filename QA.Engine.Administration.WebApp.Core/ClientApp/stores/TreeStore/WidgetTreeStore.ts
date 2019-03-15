import SiteMapService from 'services/SiteMapService';
import { BaseTreeState, ITreeElement } from 'stores/TreeStore/BaseTreeStore';
import ContextMenuType from 'enums/ContextMenuType';
import { action, observable } from 'mobx';
import TreeStoreType from 'enums/TreeStoreType';

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
        let curNodes = this.getMappedNodesById(node.id);
        do {
            curNodes.forEach((n) => {
                // zones can't have versions
                if (node.id === n.id) {
                    n.isSelected = true;
                }
                if (n.childNodes.length > 0) {
                    n.isExpanded = true;
                }
                curNodes = this.getMappedNodesById(n.parentId);
            });
        } while (curNodes.length !== 0);
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

    protected convertTree(data: WidgetModel[], key: 'searchedTreeInternal' | 'treeInternal'): void {
        if (this.searchActive) {
            super.convertTree(data, key);
            return;
        }
        const tree: ITreeElement[] = [];
        const zones: ITreeElement[] = [];
        data.forEach((x) => {
            let zoneEl = zones.find(z => z.label === x.zoneName);
            if (!zoneEl) {
                zoneEl = this.mapWidgetZoneElement(x, null, -x.id);
                tree.push(zoneEl);
                zones.push(zoneEl);
            }
            const treeItem = this.mapWidgetElement(x, zoneEl.id);
            zoneEl.childNodes.push(treeItem);
            this.convertTreeInternal(x, treeItem);
        });

        this[key] = tree;
    }

    private convertTreeInternal(el: WidgetModel, parent: ITreeElement): void {
        const zones: ITreeElement[] = [];
        el.children.forEach((x) => {
            let zoneEl = zones.find(z => z.label === x.zoneName);
            if (!zoneEl) {
                zoneEl = this.mapWidgetZoneElement(x, parent.id, -x.id);
                zones.push(zoneEl);
                parent.childNodes.push(zoneEl);
            }
            const treeItem = this.mapWidgetElement(x, zoneEl.id);
            zoneEl.childNodes.push(treeItem);
            this.convertTreeInternal(x, treeItem);
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

    private getMappedNodesById(id: number): ITreeElement[] {
        let elements = this.treeInternal;
        const result: ITreeElement[] = [];
        let loop = true;
        while (loop) {
            loop = false;
            const children: ITreeElement[] = [];
            elements.filter(x => x.id === id).forEach(y => result.push(y));
            elements.filter(x => x.childNodes.length > 0).forEach((x) => {
                x.childNodes.forEach(y => children.push(<ITreeElement>y));
            });
            loop = children.length > 0;
            elements = children;
        }
        return result;
    }

    private widgetTree: WidgetModel[];
}
