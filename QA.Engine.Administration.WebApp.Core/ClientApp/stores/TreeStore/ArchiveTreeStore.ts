import SiteMapService from 'services/SiteMapService';
import { BaseTreeState, ITreeElement } from 'stores/TreeStore/BaseTreeStore';
import ContextMenuType from 'enums/ContextMenuType';
import TreeStoreType from 'enums/TreeStoreType';
import { action, computed } from 'mobx';

export interface PageEntity {
    originalTree: ArchiveModel[];
    mappedTree: ITreeElement[];
}

export default class ArchiveTreeStore extends BaseTreeState<ArchiveModel> {
    public type = TreeStoreType.ARCHIVE;

    @action
    public async fetchTree(): Promise<void> {
        this.clear();
        const response: ApiResult<ArchiveModel[]> = await this.getTree();
        if (response.isSuccess) {
            this.origTreeInternal = response.data;
            this.handlePagination();
        } else {
            throw response.error;
        }
    }

    @computed
    get tree(): ITreeElement[] {
        return this.treeInternal;
    }

    @action
    handlePagination(number: number = null) {
        if (this.origTreeInternal.length > this.MAX_SIZE) {
            if (this.pagesCount === null) {
                this.pagesCount = Math.round(this.origTreeInternal.length / this.MAX_SIZE);
            }
            if (number !== null) {
                this.pageIndex = this.pageIndex < 0 ? -1 : number < 0 ? 0 : number;
            } else {
                this.pageIndex += 1;
            }
            const maybePage = this.pagesMap.get(this.pageIndex);
            if (maybePage) {
                this.treeInternal = maybePage.mappedTree;
            } else {
                const arr = this.origTreeInternal.splice(this.pageIndex, this.MAX_SIZE);
                this.convertTree(arr, 'treeInternal');
                this.pagesMap.set(this.pageIndex, {
                    originalTree: arr,
                    mappedTree: this.treeInternal,
                });
            }
        } else {
            this.pageIndex = -1;
            this.convertTree(this.origTreeInternal, 'treeInternal');
        }
    }

    protected contextMenuType: ContextMenuType = ContextMenuType.ARCHIVE;

    protected async getTree(): Promise<ApiResult<ArchiveModel[]>> {
        return await SiteMapService.getArchiveTree();
    }

    protected getSubTree(id: number): Promise<ApiResult<ArchiveModel>> {
        return SiteMapService.getArchiveSubTree(id);
    }

    private MAX_SIZE = 50;
    private pagesMap = new Map<number, PageEntity>();
}
