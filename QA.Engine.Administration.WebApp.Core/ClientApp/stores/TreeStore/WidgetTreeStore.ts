import SiteMapService from "services/SiteMapService";
import {BaseTreeState, ITreeElement, MapEntity} from "stores/TreeStore/BaseTreeStore";
import ContextMenuType from "enums/ContextMenuType";
import { action, observable } from "mobx";
import TreeStoreType from "enums/TreeStoreType";
import { IconName, MaybeElement, Intent, Icon, Tag } from "@blueprintjs/core";
import * as React from "react";
import NodeLabel from "components/TreeStructure/NodeLabel";

export default class WidgetTreeStore extends BaseTreeState<WidgetModel> {
    public type = TreeStoreType.WIDGET;

    @observable public selectedSiteTreeNode: PageModel;

    public get widgetsInZone(): WidgetModel[] {
        const widgets: WidgetModel[] = [];
        const widgetMaps = Array.from(this.nodesMap.values()).filter((item: MapEntity<WidgetModel>) => item.original.zoneName === this.selectedNode.zoneName);
        widgetMaps.forEach((x) => {
            widgets.push(x.original);
        });

        return widgets.filter((value, index, array) => array.indexOf(value) === index);
    }

    @action
    public handleNodeExpand = (nodeData: ITreeElement) => {
        return (nodeData.isExpanded = true);
    };

    @action
    public handleNodeCollapse = (nodeData: ITreeElement) =>
        (nodeData.isExpanded = false);

    @action
    public init(selectedNode: any) {
        this.resetSearch();
        this.resetDiscriminators();
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
    public setSelectedNode = (node: ITreeElement) => {
        const targetNode = this.nodesMap.get(node.id);
        this.selectedNode = targetNode.original;
        this.selectedTreeElement = node;
    };

    @action
    public getWidgetDiscriminators() {
        return this.widgetDiscriminators;
    }

    @action
    public getAlias = (node: ITreeElement) => {
        const targetNode = this.nodesMap.get(node.id);
        return targetNode.original.alias;
    };

    @action
    public getNodeToScroll(): number {
        if (this.expandLaunched && this.cordsUpdated && !this.searchActive) {
            this.setCordsUpdateStatus(false);
            this.expandLaunched = false;
            return this.selectedTreeElement.id;
        }
        return null;
    }

    protected readonly contextMenuType: ContextMenuType =
        ContextMenuType.WIDGET;

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

    protected getIcon = (el: WidgetModel): IconName | MaybeElement => {
        if (el.id < 0) {
            return this.icons.node;
        }
        const discriminator =
            this.discriminators == null
                ? null
                : this.discriminators.filter(
                      (x) => x.id === el.discriminatorId
                  )[0];
        if (discriminator != null) {
            const iconProps = {
                icon: <IconName>discriminator.iconClass,
                intent: <Intent>discriminator.iconIntent,
                className: "bp3-tree-node-icon",
            };
            const tagProps = {
                minimal: true,
                intent: Intent.SUCCESS,
                className: "bp3-tree-node-icon",
            };
            const icon = React.createElement(Icon, iconProps);
            const tag = React.createElement(Tag, tagProps, "new");

            if (!el.published) {
                return React.createElement(React.Fragment, {}, icon, tag);
            }
            return icon;
        }
        if (this.icons.checkPublication) {
            if (this.searchActive) {
                return el.published
                    ? this.icons.leafPublished
                    : this.icons.leaf;
            }
            if (el.parentId === null) {
                return el.published
                    ? this.icons.rootPublished
                    : this.icons.root;
            }
            if (!el.children || el.children.length === 0) {
                return el.published
                    ? this.icons.leafPublished
                    : this.icons.leaf;
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
    };

    protected searchInternal(
        results: Set<WidgetModel>,
        query: string,
        node: WidgetModel
    ) {
        if (node.zoneName && node.zoneName.toLowerCase().includes(query)) {
            const foundEl: WidgetModel = {
                ...node,
                id: -node.id,
                title: node.zoneName,
                children: [],
                parentId: null,
            };
            results.add(foundEl);
        }
    }

    protected searchDiscriminatorInternal(results: Set<WidgetModel>, id: number, node: WidgetModel) {
        if (node.discriminatorId === id) {
            const foundEl: WidgetModel = {
                ...node,
                id: -node.id,
                title: node.zoneName,
                children: [],
                parentId: null,
            };
            results.add(foundEl);
        }
    }

    protected convertTree(
        data: WidgetModel[],
        key: "searchedTreeInternal" | "treeInternal"
    ): void {
        if (key === "searchedTreeInternal") {
            super.convertTree(data, key);
            return;
        }
        const tree: ITreeElement[] = [];
        const zones: ITreeElement[] = [];

        data.forEach((x) => {
            const path: string[] = [];
            path.push(`[${x.zoneName ? x.zoneName : ""}]`, x.title);
            let zoneEl = zones.find((z) => z.title === x.zoneName);
            if (!zoneEl) {
                zoneEl = this.mapWidgetZoneElement(x, null, -x.id);
                this.nodesMap.set(zoneEl.id, { original: x, mapped: zoneEl });
                tree.push(zoneEl);
                zones.push(zoneEl);
            }
            const treeItem = this.mapWidgetElement(x, zoneEl.id);
            zoneEl.childNodes.push(treeItem);
            this.nodesMap.set(x.id, { original: x, mapped: treeItem });
            this.pathMap.set(treeItem.id, path.join("/"));
            this.pathMap.set(-treeItem.id, path.join("/"));
            path.push(x.title);
            this.convertTreeInternal(x, treeItem, path);
        });

        this[key] = tree;
    }

    private convertTreeInternal(
        el: WidgetModel,
        parent: ITreeElement,
        path: string[]
    ): void {
        const zones: ITreeElement[] = [];
        el.children.forEach((x) => {
            const internalPath = Object.assign([], path);
            internalPath.push(`[${x.zoneName ? x.zoneName : ""}]`);
            let zoneEl = zones.find((z) => z.title === x.zoneName);
            if (!zoneEl) {
                zoneEl = this.mapWidgetZoneElement(x, parent.id, -x.id);
                this.nodesMap.set(zoneEl.id, { original: x, mapped: zoneEl });
                zones.push(zoneEl);
                parent.childNodes.push(zoneEl);
            }
            const treeItem = this.mapWidgetElement(x, zoneEl.id);
            zoneEl.childNodes.push(treeItem);
            this.nodesMap.set(x.id, { original: x, mapped: treeItem });
            this.pathMap.set(treeItem.id, internalPath.join("/"));
            this.pathMap.set(-treeItem.id, internalPath.join("/"));
            internalPath.push(x.title);
            this.convertTreeInternal(x, treeItem, internalPath);
        });
    }

    private mapWidgetElement(
        el: WidgetModel,
        parentId?: number,
        id?: number
    ): ITreeElement {
        const treeElement = super.mapElement(el);
        // if (this.icons.checkPublication) {
        //     treeElement.icon = el.published ? this.icons.leafPublished : this.icons.leaf;
        // } else {
        //     treeElement.icon = this.icons.leaf;
        // }
        if (parentId != null) {
            treeElement.parentId = parentId;
        }
        if (id != null) {
            treeElement.id = id;
        }
        return treeElement;
    }

    private mapWidgetZoneElement(
        x: WidgetModel,
        parentId?: number,
        id?: number
    ): ITreeElement {
        const treeElement = observable.object<ITreeElement>(
            {
                id: id || x.id,
                childNodes: [],
                label: "",
                secondaryLabel: "",
                title: x.zoneName,
                isExpanded: false,
                icon: this.icons.node,
                hasCaret: true,
                isContextMenuActive: false,
                parentId: parentId || x.parentId,
                contextMenuType: null,
                visible: x.visible,
                isPublished: x.published,
                isSelected: false,
                disabled: false,
            },
            {
                label: observable.ref,
                secondaryLabel: observable.ref,
            },
            {
                deep: false,
            }
        );
        treeElement.label = React.createElement(NodeLabel, {
            node: treeElement,
            type: this.type,
        });

        return treeElement;
    }

    private widgetTree: WidgetModel[];
    private selectedTreeElement: ITreeElement;
}
