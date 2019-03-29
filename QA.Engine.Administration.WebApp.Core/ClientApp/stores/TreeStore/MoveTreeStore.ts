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
        this.origTreeInternal = JSON.parse(JSON.stringify(origTree)) as PageModel[];
        const node = this.getNodeById(selectedNode.id);
        if (node.parentId == null) {
            const index = this.getIndex(this.origTreeInternal, selectedNode.id);
            if (index > -1) {
                this.origTreeInternal.splice(index, 1);
            }
        } else {
            const parentNode = this.getNodeById(node.parentId);
            const index = this.getIndex(parentNode.children, selectedNode.id);
            if (index > -1) {
                parentNode.children.splice(index, 1);
            }
        }

        this.convertTree(this.origTreeInternal, 'treeInternal');
        this.selectedNode = null;
    }

    protected readonly contextMenuType: ContextMenuType = null;
    protected getTree = (): Promise<ApiResult<PageModel[]>> => null;
    protected getSubTree = (): Promise<ApiResult<PageModel>> => null;

    private getIndex = (elements: PageModel[], id: number) => {
        for (let i = 0; i < elements.length; i += 1) {
            if (elements[i].id === id) {
                return i;
            }
        }
        return -1;
    }
}
