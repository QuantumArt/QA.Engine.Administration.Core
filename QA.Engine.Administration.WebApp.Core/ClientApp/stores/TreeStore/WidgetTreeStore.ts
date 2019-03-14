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
                if (node.id === n.id && n.versionOfId !== undefined) {
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
        let elements = data;

        let hMap = new Map<number, ITreeElement>();
        const tree: ITreeElement[] = [];
        elements.forEach((x) => {
            const zoneItem = tree.find(t => t.label === x.zoneName); // TODO:???
            const treeItem = this.mapElement(x);
            hMap.set(x.id, treeItem);
            if (zoneItem == null) {
                const item = this.mapWidgetZoneElement(x);
                item.childNodes.push(treeItem);
                tree.push(item);
            } else {
                zoneItem.childNodes.push(treeItem);
            }
        });

        let loop = true;
        while (loop) {
            loop = false;
            const childNodes = new Map<number, ITreeElement>();
            let children: WidgetModel[] = [];
            elements.forEach((x) => {
                if (x.hasChildren) {
                    children = children.concat(x.children);
                }
            });
            children.forEach((x) => {
                loop = true;
                const zoneEl = this.mapWidgetZoneElement(x);
                const el = this.mapWidgetElement(x, zoneEl.parentId);
                const parentId = el.parentId == null ? el.versionOfId : el.parentId;
                const treeEl = hMap.get(parentId);
                if (treeEl) {
                    const zone = treeEl.childNodes.find(t => t.label === x.zoneName);
                    if (!zone) {
                        zoneEl.childNodes.push(el);
                        treeEl.childNodes.push(zoneEl);
                    } else {
                        zone.childNodes.push(el);
                    }
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

    private mapWidgetElement(el: WidgetModel, id?: number): ITreeElement {
        const treeElement = super.mapElement(el);
        if (this.icons.checkPublication) {
            treeElement.icon = el.published ? this.icons.leafPublished : this.icons.leaf;
        } else {
            treeElement.icon = this.icons.leaf;
        }
        if (id) {
            treeElement.parentId = id;
        }
        return treeElement;
    }

    private mapWidgetZoneElement(x: WidgetModel): ITreeElement {
        return observable<ITreeElement>({
            id: x.id,
            childNodes: [],
            label: x.zoneName,
            isExpanded: false,
            icon: this.icons.node,
            hasCaret: true,
            isContextMenuActive: false,
            parentId: x.parentId,
            contextMenuType: null,
            isVisible: x.isVisible,
            isPublished: x.published,
        });
    }

    private getMappedNodesById(id: ITreeElement['id']): ITreeElement[] {
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
