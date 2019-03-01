import v4 from 'uuid/v4';
import SiteMapService from 'services/SiteMapService';
import { BaseTreeState, ITreeElement } from 'stores/TreeStore/BaseTreeStore';
import ContextMenuType from 'enums/ContextMenuType';
import OperationState from 'enums/OperationState';
import { action, observable } from 'mobx';
import { TreeErrors } from 'enums/ErrorsTypes';

export default class WidgetTreeStore extends BaseTreeState<WidgetModel> {

    @observable public selectedSiteTreeNode: PageModel;

    @action
    public handleNodeExpand = (nodeData: ITreeElement) => nodeData.isExpanded = true

    @action
    public handleNodeCollapse = (nodeData: ITreeElement) => nodeData.isExpanded = false

    @action
    public init(selectedNode: any) {
        this.selectedSiteTreeNode = selectedNode;
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

    protected convertTree(data: WidgetModel[]): void {
        let elements = data;
        let loop = true;

        let hMap = new Map<number, ITreeElement>();
        const tree: ITreeElement[] = [];
        elements.forEach((x) => {
            const zoneItem = tree.find(t => t.label === x.zoneName);
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
                const el = this.mapElement(x);
                const parentId = el.parentId == null ? el.versionOfId : el.parentId;
                const treeEl = hMap.get(parentId);
                if (treeEl != null) {
                    const zone = treeEl.childNodes.find(t => t.label === x.zoneName);
                    if (zone == null) {
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
        this.treeInternal = tree;
    }

    protected mapElement(el: WidgetModel): ITreeElement {
        const treeElement = super.mapElement(el);
        if (this.icons.checkPublication) {
            treeElement.icon = el.published ? this.icons.leafPublished : this.icons.leaf;
        } else {
            treeElement.icon = this.icons.leaf;
        }
        return treeElement;
    }

    private mapWidgetZoneElement(x: WidgetModel): ITreeElement {
        return observable<ITreeElement>({
            id: x.zoneName,
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

    private widgetTree: WidgetModel[];
}
