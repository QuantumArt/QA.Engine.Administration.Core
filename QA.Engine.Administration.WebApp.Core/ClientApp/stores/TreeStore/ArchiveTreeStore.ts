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
                this.pagesCount = Math.ceil(this.origTreeInternal.length / this.MAX_SIZE);
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

    @action
    public search(query: string) {
        if (this.selectedNode != null) {
            const maybeCurNode = this.nodesMap.get(this.selectedNode.id);
            if (maybeCurNode) {
                maybeCurNode.mapped.isSelected = false;
            }
        }
        this.selectedNode = null;
        super.search(query);
    }

    @action
    public handleNodeClick = (nodeData: ITreeElement) => {
        const targetNode = this.nodesMap.get(nodeData.id) || this.searchedNodesMap.get(nodeData.id);
        const originallySelected = nodeData.isSelected;
        if (this.selectedNode != null) {
            if (this.searchActive) {
                const node = this.searchedNodesMap.get(this.selectedNode.id);
                if (node) {
                    node.mapped.isSelected = false;
                }
            } else {
                const node = this.nodesMap.get(this.selectedNode.id);
                if (node) {
                    node.mapped.isSelected = false;
                }
            }
        }
        this.selectedNode = nodeData.isSelected === true ? null : targetNode.original;
        nodeData.isSelected = originallySelected == null ? true : !originallySelected;
    }

    @action
    public clear() {
        super.clear();
        this.pagesMap.clear();
        this.pageIndex = -1;
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
