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
import MoveTreeStore from './MoveTreeStore';

export type TreeType = SiteTreeStore | ArchiveTreeStore | ContentVersionTreeStore | WidgetTreeStore | MoveTreeStore;

/**
 * @description Facade class to access a proper tree
 */
export default class TreeStore extends ErrorHandler {

    private readonly siteTreeStore: SiteTreeStore;
    private readonly archiveStore: ArchiveTreeStore;
    private readonly contentVersionsStore: ContentVersionTreeStore;
    private readonly widgetStore: WidgetTreeStore;
    private readonly moveStore: MoveTreeStore;
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
        this.moveStore = new MoveTreeStore();
        this.navigationStore = navigationStore;
    }

    @observable public state: OperationState = OperationState.NONE;

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
    getMoveTreeStore(): MoveTreeStore {
        return this.moveStore;
    }

    resolveMainTreeStore(): ArchiveTreeStore | SiteTreeStore {
        if (this.navigationStore.currentPage === Pages.ARCHIVE) {
            return this.archiveStore;
        }
        return this.siteTreeStore;
    }

    @action
    async updateSubTree(id?: number): Promise<any> {
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
            [this.contentVersionsStore, this.widgetStore].forEach(x => x.init(selectedNode));
        }
    }

    @action
    async fetchTree(): Promise<any> {
        const store = this.resolveMainTreeStore();
        await this.runAsync(
            async () => {
                await store.fetchTree();
                this.state = OperationState.SUCCESS;
            },
            ErrorsTypes.Tree.fetch);
    }

    @action
    async publish(itemIds: number[]): Promise<any> {
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
    async archive(model: RemoveModel): Promise<any> {
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
    async edit(model: EditModel): Promise<any> {
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
    async reorder(model: ReorderModel): Promise<any> {
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
    async move(model: MoveModel): Promise<any> {
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
    async restore(model: RestoreModel): Promise<any> {
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
    async delete(model: DeleteModel): Promise<any> {
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
