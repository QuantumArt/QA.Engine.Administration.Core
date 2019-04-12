import ArchiveTreeStore from './ArchiveTreeStore';
import SiteTreeStore from './SiteTreeStore';
import NavigationStore, { Pages } from '../NavigationStore';
import ContentVersionTreeStore from './ContentVersionTreeStore';
import WidgetTreeStore from './WidgetTreeStore';
import { action, observable } from 'mobx';
import OperationState from 'enums/OperationState';
import ErrorsTypes from 'constants/ErrorsTypes';
import SiteMapService from 'services/SiteMapService';
import ErrorHandler from 'stores/ErrorHandler';
import TreeStoreType from 'enums/TreeStoreType';

export type TreeType = SiteTreeStore | ArchiveTreeStore | ContentVersionTreeStore | WidgetTreeStore;
export type TreeStructureType = 'main' | 'widgets' | 'versions';

/**
 * @description Facade class to access a proper tree
 */
export default class TreeStore extends ErrorHandler {

    private readonly siteTreeStore: SiteTreeStore;
    private readonly archiveStore: ArchiveTreeStore;
    private readonly contentVersionsStore: ContentVersionTreeStore;
    private readonly widgetStore: WidgetTreeStore;
    private readonly navigationStore: NavigationStore;

    constructor(navigationStore: NavigationStore) {
        super();
        this.siteTreeStore = new SiteTreeStore();
        this.archiveStore = new ArchiveTreeStore();
        this.contentVersionsStore = new ContentVersionTreeStore({
            checkPublication: true,
            root: 'document',
            rootPublished: 'saved',
            node: 'document',
            nodePublished: 'saved',
            nodeOpen: 'document',
            nodeOpenPublished: 'saved',
            leaf: 'document',
            leafPublished: 'saved',
        });
        this.widgetStore = new WidgetTreeStore({
            checkPublication: true,
            root: 'application',
            node: 'widget',
            nodePublished: 'widget',
            nodeOpen: 'widget',
            nodeOpenPublished: 'widget',
            leaf: 'document',
            leafPublished: 'saved',
        });
        this.navigationStore = navigationStore;
    }

    @observable public state: OperationState = OperationState.NONE;

    public getSiteTreeStore(): SiteTreeStore {
        return this.siteTreeStore;
    }
    public getArchiveTreeStore(): ArchiveTreeStore {
        return this.archiveStore;
    }
    public getContentVersionTreeStore(): ContentVersionTreeStore {
        return this.contentVersionsStore;
    }
    public getWidgetTreeStore(): WidgetTreeStore {
        return this.widgetStore;
    }
    public resolveMainTreeStore(): ArchiveTreeStore | SiteTreeStore {
        if (this.navigationStore.currentPage === Pages.ARCHIVE) {
            return this.archiveStore;
        }
        return this.siteTreeStore;
    }

    public resolveTree = (type: TreeStructureType | TreeStoreType): TreeType => {
        switch (type) {
            case 'main':
            case TreeStoreType.SITE:
            case TreeStoreType.ARCHIVE:
                return this.resolveMainTreeStore();
            case 'widgets':
            case TreeStoreType.WIDGET:
                return this.getWidgetTreeStore();
            case 'versions':
            case TreeStoreType.CONTENTVERSION:
                return this.getContentVersionTreeStore();
            default:
                return null;
        }
    }

    @action
    public async updateSubTree(id?: number): Promise<any> {
        let current = this.resolveMainTreeStore();
        let selectedNode = current.selectedNode;
        await this.runAsync(
            async () => {
                await current.updateSubTree(id == null ? selectedNode.id : id);
                this.state = OperationState.SUCCESS;
            },
            ErrorsTypes.Tree.update,
            id == null ? selectedNode.id : id);

        current = this.resolveMainTreeStore();
        if (current instanceof SiteTreeStore) {
            selectedNode = current.selectedNode;
        }
    }

    @action
    public async fetchTree(): Promise<any> {
        const store = this.resolveMainTreeStore();
        await this.runAsync(
            async () => {
                await store.fetchTree();
                this.state = OperationState.SUCCESS;
            },
            ErrorsTypes.Tree.fetch);
    }

    @action
    public async publish(itemIds: number[]): Promise<any> {
        await this.runAsync(
            async () => {
                const response: ApiResult<any> = await SiteMapService.publish(itemIds);
                if (response.isSuccess) {
                    await this.updateSubTree();
                    this.state = OperationState.SUCCESS;
                } else {
                    throw response.error;
                }
            },
            ErrorsTypes.Tree.publish,
            itemIds);
    }

    @action
    public async archive(model: RemoveModel): Promise<any> {
        await this.runAsync(
            async () => {
                const response: ApiResult<any> = await SiteMapService.archive(model);
                if (response.isSuccess) {
                    await this.updateSubTree();
                    this.state = OperationState.SUCCESS;
                } else {
                    throw response.error;
                }
            },
            ErrorsTypes.Tree.archive,
            model);
    }

    @action
    public async edit(model: EditModel): Promise<any> {
        await this.runAsync(
            async () => {
                const response: ApiResult<any> = await SiteMapService.edit(model);
                if (response.isSuccess) {
                    await this.updateSubTree();
                    this.state = OperationState.SUCCESS;
                } else {
                    throw response.error;
                }
            },
            ErrorsTypes.Tree.edit,
            model);
    }

    @action
    public async reorder(model: ReorderModel): Promise<any> {
        await this.runAsync(
            async () => {
                const response: ApiResult<any> = await SiteMapService.reorder(model);
                if (response.isSuccess) {
                    const store = this.siteTreeStore;
                    const parentId = store.selectedNode.parentId;
                    await this.updateSubTree(parentId);
                    this.state = OperationState.SUCCESS;
                } else {
                    throw response.error;
                }
            },
            ErrorsTypes.Tree.reorder,
            model);
    }

    @action
    public async move(model: MoveModel): Promise<any> {
        await this.runAsync(
            async () => {
                const response: ApiResult<any> = await SiteMapService.move(model);
                if (response.isSuccess) {
                    const store = this.siteTreeStore;
                    const parentId = store.selectedNode.parentId;
                    await this.updateSubTree(parentId);
                    await this.updateSubTree(model.newParentId);
                    this.state = OperationState.SUCCESS;
                } else {
                    throw response.error;
                }
            },
            ErrorsTypes.Tree.move,
            model);
    }

    @action
    public async restore(model: RestoreModel): Promise<any> {
        await this.runAsync(
            async () => {
                const response: ApiResult<any> = await SiteMapService.restore(model);
                if (response.isSuccess) {
                    await this.fetchTree();
                    this.state = OperationState.SUCCESS;
                } else {
                    throw response.error;
                }
            },
            ErrorsTypes.Tree.restore,
            model);
    }

    @action
    public async delete(model: DeleteModel): Promise<any> {
        await this.runAsync(
            async () => {
                const response: ApiResult<any> = await SiteMapService.delete(model);
                if (response.isSuccess) {
                    await this.updateSubTree();
                    this.state = OperationState.SUCCESS;
                } else {
                    throw response.error;
                }
            },
            ErrorsTypes.Tree.delete,
            model);
    }

    @action
    private async runAsync(func: () => Promise<void>, treeErrors: ErrorsTypes, model?: any): Promise<void> {
        this.state = OperationState.PENDING;
        try {
            await func();
        } catch (e) {
            this.state = OperationState.ERROR;
            this.addError(treeErrors, model, e);
        }
    }
}
