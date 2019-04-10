import { action } from 'mobx';
import { BaseTreeState } from 'stores/TreeStore/BaseTreeStore';
import ContextMenuType from 'enums/ContextMenuType';
import TreeStoreType from 'enums/TreeStoreType';

export default class MoveTreeStore extends BaseTreeState<PageModel> {

    public type = TreeStoreType.MOVE;

    @action
    public init(selectedNode: PageModel, origTree: PageModel[]) {
        if (selectedNode == null) {
            return;
        }
        this.origTreeInternal = origTree;

        this.convertTree(this.origTreeInternal, 'treeInternal');
        if (this.nodesMap.has(selectedNode.id)) {
            const mapEntity = this.nodesMap.get(selectedNode.id);
            this.expandToNode(mapEntity.mapped);
            mapEntity.mapped.isExpanded = false;
            mapEntity.mapped.isSelected = false;
            mapEntity.mapped.disabled = true;
        }
        this.selectedNode = null;
    }

    protected readonly contextMenuType: ContextMenuType = null;
    protected getTree = (): Promise<ApiResult<PageModel[]>> => null;
    protected getSubTree = (): Promise<ApiResult<PageModel>> => null;
}
