import SiteMapService from "services/SiteMapService";
import { BaseTreeState, ITreeElement } from "stores/TreeStore/BaseTreeStore";
import ContextMenuType from "enums/ContextMenuType";
import TreeStoreType from "enums/TreeStoreType";
import DictionaryService from "services/DictionaryService";
import { action, computed, observable } from "mobx";

export default class SiteTreeStore extends BaseTreeState<PageModel> {
    public type = TreeStoreType.SITE;
    public customerOptions: CustomerOptionsModel = null;

    private regionIds: number[] = [];

    public get parentNode(): PageModel {
        return this.nodesMap.get(this.selectedNode.parentId).original;
    }

    public setRegions(regionId?: number) {
        this.resetSearch();
        this.regionIds = regionId == null ? [] : [regionId];
    }

    public async getRootElement(): Promise<PageModel> {
        await this.getRootPageDiscriminator();
        let node = this.selectedNode;
        while (
            node.parentId != null &&
            node.discriminator !== this.rootPageDiscriminator
        ) {
            node = this.nodesMap.get(node.parentId).original;
        }
        return node;
    }

    protected contextMenuType: ContextMenuType = ContextMenuType.SITEMAP;

    protected async getTree(): Promise<ApiResult<PageModel[]>> {
        await this.getCustomerOptions();
        if (this.regionIds.length === 0) {
            return await SiteMapService.getSiteMapTree();
        }
        return await SiteMapService.getSiteMapTree(this.regionIds);
    }

    protected async getSubTree(id: number): Promise<ApiResult<PageModel>> {
        return await SiteMapService.getSiteMapSubTree(id, this.regionIds);
    }

    private rootPageDiscriminator: string = null;
    private async getRootPageDiscriminator() {
        try {
            const response: ApiResult<string> =
                await DictionaryService.getRootPageDiscriminator();
            if (response.isSuccess) {
                this.rootPageDiscriminator = response.data;
            } else {
                this.rootPageDiscriminator = null;
            }
        } catch (e) {
            this.rootPageDiscriminator = null;
        }
    }

    @observable private moveTreeModeInternal: boolean = false;
    @computed
    public get moveTreeMode(): boolean {
        return this.moveTreeModeInternal;
    }

    private moveItemIdInternal: number;
    public get moveItemId(): number {
        return this.moveItemIdInternal;
    }
    public get treeElement(): ITreeElement {
        if (
            this.selectedNode != null &&
            this.nodesMap.has(this.selectedNode.id)
        ) {
            return this.nodesMap.get(this.selectedNode.id).mapped;
        }
        return null;
    }

    @action
    public getSiteDiscriminators() {
        return this.siteDiscriminators;
    }

    @action
    public startMoveTree() {
        this.moveTreeModeInternal = true;
        this.moveItemIdInternal = this.selectedNode.id;
        if (this.nodesMap.has(this.moveItemIdInternal)) {
            const mapEntity = this.nodesMap.get(this.moveItemIdInternal);
            mapEntity.mapped.isExpanded = false;
            mapEntity.mapped.isSelected = false;
            mapEntity.mapped.disabled = true;
        }
    }

    @action
    public cancelMoveTree() {
        if (this.nodesMap.has(this.moveItemIdInternal)) {
            const mapEntity = this.nodesMap.get(this.moveItemIdInternal);
            mapEntity.mapped.isSelected = true;
            mapEntity.mapped.disabled = false;
            if (
                this.selectedNode != null &&
                this.selectedNode.id !== mapEntity.original.id
            ) {
                this.nodesMap.get(this.selectedNode.id).mapped.isSelected =
                    false;
                this.selectedNode = mapEntity.original;
                this.expandToNode(mapEntity.mapped);
            }
        }
        this.moveItemIdInternal = null;
        this.moveTreeModeInternal = false;
        if (this.query !== "") {
            this.search("");
        }
    }

    protected searchInternal(
        results: Set<PageModel>,
        query: string,
        node: PageModel
    ) {
        let del: PageModel;
        results.forEach((x) => {
            if (x.id === this.moveItemIdInternal) {
                del = x;
            }
        });
        if (del) {
            results.delete(del);
        }
    }

    public searchDiscriminatorInternal(
        results: Set<PageModel>,
        id: number,
        node: PageModel
    ) {
        let del: PageModel;
        results.forEach((x) => {
            if (x.id === this.moveItemIdInternal) {
                del = x;
            }
        });
        if (del) {
            results.delete(del);
        }
    }

    public getAlias = (node: ITreeElement) => {
        const targetNode = this.nodesMap.get(node.id);
        return targetNode.original.alias;
    };

    public getNode = (id: number) => {
        return this.nodesMap.get(id).original;
    };

    public async getPathToElement(id: number): Promise<string> {
        const path: string[] = [];
        await this.getRootPageDiscriminator();

        let node = this.nodesMap.get(id).original;

        while (
            node.parentId != null &&
            node.discriminator !== this.rootPageDiscriminator
        ) {
            path.push(node.alias);
            node = this.nodesMap.get(node.parentId).original;
        }
        return path.reverse().join('/');
    }

    private async getCustomerOptions() {
        try {
            const response: ApiResult<CustomerOptionsModel> = await DictionaryService.getCustomerOptions();
            if (response.isSuccess) {
                this.customerOptions = response.data;
            } else {
                this.customerOptions = null;
            }
        } catch (e) {
            this.customerOptions = null;
        }
    }
}
