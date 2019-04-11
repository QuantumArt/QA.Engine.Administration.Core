import { action, observable, computed } from 'mobx';
import { BaseTreeState, MapEntity } from 'stores/TreeStore/BaseTreeStore';
import ContextMenuType from 'enums/ContextMenuType';
import TreeStoreType from 'enums/TreeStoreType';

export default class MoveTreeStore extends BaseTreeState<PageModel> {

    public type = TreeStoreType.MOVE;
    @observable isLoading: boolean;

    @computed
    get loading(): boolean {
        return this.isLoading;
    }

    @action
    public init(selectedNode: PageModel, origTree: PageModel[]) {
        if (selectedNode == null) {
            return;
        }
        this.isLoading = true;
        setTimeout(() => {
            this.origTreeInternal = JSON.parse(JSON.stringify(origTree));

            this.convertTree(this.origTreeInternal, 'treeInternal');
            if (this.nodesMap.has(selectedNode.id)) {
                const mapEntity = this.nodesMap.get(selectedNode.id);
                this.expandToNode(mapEntity.mapped);
                mapEntity.mapped.isExpanded = false;
                mapEntity.mapped.isSelected = false;
                mapEntity.mapped.disabled = true;
            }
            this.selectedNode = null;
            this.isLoading = false;
        },         0);
    }

    protected readonly contextMenuType: ContextMenuType = null;
    protected getTree = (): Promise<ApiResult<PageModel[]>> => null;
    protected getSubTree = (): Promise<ApiResult<PageModel>> => null;

    clear() {
        this.showIDs = false;
        this.showPath = false;
        this.searchActive = false;
        this.query = '';
        this.cordsUpdated = false;
        this.expandLaunched = false;
        this.searchTimer = null;
        this.selectedNode = null;
        this.nodeCords = new Map<number, number>();
        this.pathMap = new Map<number, string>();
        this.treeInternal = [];
        this.searchedTreeInternal = [];
        this.nodesMap = new Map<number, MapEntity<PageModel>>();
        this.searchedNodesMap = new Map<number, MapEntity<PageModel>>();
        this.origTreeInternal = [];
        this.origSearchedTreeInternal = [];
    }
}
