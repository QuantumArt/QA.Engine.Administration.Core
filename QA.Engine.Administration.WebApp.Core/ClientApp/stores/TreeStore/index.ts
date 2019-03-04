import ArchiveTreeStore from './ArchiveTreeStore';
import SiteTreeStore from './SiteTreeStore';
import NavigationStore, { Pages } from '../NavigationStore';
import ContentVersionTreeStore from './ContentVersionTreeStore';
import WidgetTreeStore from './WidgetTreeStore';
import { observable, action, computed } from 'mobx';
import OperationState from 'enums/OperationState';
import TreeStoreType from 'enums/TreeStoreType';
import ErrorsTypes from 'constants/ErrorsTypes';
import SiteMapService from 'services/SiteMapService';
import ErrorHandler from 'stores/ErrorHandler';

export type TreeType = SiteTreeStore | ArchiveTreeStore | ContentVersionTreeStore | WidgetTreeStore;

/**
 * @description Facade class to access a proper tree
 */
export default class TreeStore extends ErrorHandler {

    private readonly siteTreeStore: SiteTreeStore;
    private readonly archiveStore: ArchiveTreeStore;
    private readonly contentVersionsStore: ContentVersionTreeStore;
    private readonly widgetStore: WidgetTreeStore;
    private readonly navigationStore: NavigationStore;

    private readonly treeState: TreeState;
    private readonly stores: Map<TreeStoreType, TreeType>;

    constructor(navigationStore: NavigationStore) {
        super();
        this.siteTreeStore = new SiteTreeStore();
        this.archiveStore = new ArchiveTreeStore();
        this.contentVersionsStore = new ContentVersionTreeStore({
            checkPublication: true,
            root: 'document',
            rootPublished: 'saved',
        });
        this.widgetStore = new WidgetTreeStore({
            checkPublication: true,
            root: 'application',
            node: 'widget',
            nodePublished: 'widget',
            nodeOpen: 'widget',
            nodeOpenPublished: 'saved',
            leaf: 'document',
            leafPublished: 'saved',
        });

        this.treeState = new TreeState();
        this.stores = new Map<TreeStoreType, TreeType>();
        this.stores.set(TreeStoreType.SITE, this.siteTreeStore);
        this.stores.set(TreeStoreType.ARCHIVE, this.archiveStore);
        this.stores.set(TreeStoreType.CONTENTVERSION, this.contentVersionsStore);
        this.stores.set(TreeStoreType.WIDGET, this.widgetStore);

        this.navigationStore = navigationStore;
    }

    @computed
    get state(): OperationState {
        return this.treeState.state;
    }

    getSiteTreeStore(): SiteTreeStore {
        return this.siteTreeStore;
    }
    getArchiveTreeStore(): ArchiveTreeStore {
        return this.archiveStore;
    }
    getContentVersionTreeStore(): ContentVersionTreeStore {
        return this.contentVersionsStore;
    }
    getWidgetTreeStore(): WidgetTreeStore {
        return this.widgetStore;
    }

    resolveMainTreeStore(): ArchiveTreeStore | SiteTreeStore {
        if (this.navigationStore.currentPage === Pages.ARCHIVE) {
            return this.archiveStore;
        }
        return this.siteTreeStore;
    }

    async updateSubTree(id?: number): Promise<any> {
        let current = this.resolveMainTreeStore();
        let selectedNode = current.selectedNode;
        await this.runAsync(
            async () => {
                await current.updateSubTree(id == null ? selectedNode.id : id);
                this.treeState.success();
            },
            ErrorsTypes.Tree.update,
            id == null ? selectedNode.id : id);

        current = this.resolveMainTreeStore();
        if (current instanceof SiteTreeStore) {
            selectedNode = current.selectedNode;
            [this.contentVersionsStore, this.widgetStore].forEach(x => x.init(selectedNode));
        }
    }

    async fetchTree(): Promise<any> {
        const store = this.resolveMainTreeStore();
        this.runAsync(
            async () => {
                await store.fetchTree();
                this.treeState.success();
            },
            ErrorsTypes.Tree.fetch);
    }

    async publish(itemIds: number[]): Promise<any> {
        this.runAsync(
            async () => {
                const response: ApiResult<any> = await SiteMapService.publish(itemIds);
                if (response.isSuccess) {
                    await this.updateSubTree();
                    this.treeState.success();
                } else {
                    throw response.error;
                }
            },
            ErrorsTypes.Tree.publish,
            itemIds);
    }

    async archive(model: RemoveModel): Promise<any> {
        this.runAsync(
            async () => {
                const response: ApiResult<any> = await SiteMapService.archive(model);
                if (response.isSuccess) {
                    await this.updateSubTree();
                    this.treeState.success();
                } else {
                    throw response.error;
                }
            },
            ErrorsTypes.Tree.archive,
            model);
    }

    async edit(model: EditModel): Promise<any> {
        this.runAsync(
            async () => {
                const response: ApiResult<any> = await SiteMapService.edit(model);
                if (response.isSuccess) {
                    await this.updateSubTree();
                    this.treeState.success();
                } else {
                    throw response.error;
                }
            },
            ErrorsTypes.Tree.edit,
            model);
    }

    async reorder(model: ReorderModel): Promise<any> {
        this.runAsync(
            async () => {
                const response: ApiResult<any> = await SiteMapService.reorder(model);
                if (response.isSuccess) {
                    const store = this.stores.get(TreeStoreType.SITE);
                    const parentId = store.selectedNode.parentId;
                    await this.updateSubTree(parentId);
                    this.treeState.success();
                } else {
                    throw response.error;
                }
            },
            ErrorsTypes.Tree.reorder,
            model);
    }

    async move(model: MoveModel): Promise<any> {
        this.runAsync(
            async () => {
                const response: ApiResult<any> = await SiteMapService.move(model);
                if (response.isSuccess) {
                    const store = this.stores.get(TreeStoreType.SITE);
                    const parentId = store.selectedNode.parentId;
                    await this.updateSubTree(parentId);
                    await this.updateSubTree(model.newParentId);
                    this.treeState.success();
                } else {
                    throw response.error;
                }
            },
            ErrorsTypes.Tree.move,
            model);
    }

    async restore(model: RestoreModel): Promise<any> {
        this.runAsync(
            async () => {
                const response: ApiResult<any> = await SiteMapService.restore(model);
                if (response.isSuccess) {
                    await this.updateSubTree(model.itemId);
                    this.treeState.success();
                } else {
                    throw response.error;
                }
            },
            ErrorsTypes.Tree.restore,
            model);
    }

    async delete(model: DeleteModel): Promise<any> {
        this.runAsync(
            async () => {
                const response: ApiResult<any> = await SiteMapService.delete(model);
                if (response.isSuccess) {
                    await this.updateSubTree();
                    this.treeState.success();
                } else {
                    throw response.error;
                }
            },
            ErrorsTypes.Tree.delete,
            model);
    }

    private async runAsync(func: () => Promise<void>, treeErrors: ErrorsTypes, model?: any): Promise<void> {
        this.treeState.pending();
        try {
            await func();
        } catch (e) {
            this.treeState.error();
            this.addError(treeErrors, model, e);
        }
    }
}

class TreeState {
    @observable state: OperationState = OperationState.NONE;

    @action
    pending(): void {
        this.state = OperationState.PENDING;
    }

    @action
    success(): void {
        this.state = OperationState.SUCCESS;
    }

    @action
    error(): void {
        this.state = OperationState.ERROR;
    }
}
