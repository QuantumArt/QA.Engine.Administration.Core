import { BaseTreeState, ITreeElement } from 'stores/TreeStore/BaseTreeStore';
import ContextMenuType from 'enums/ContextMenuType';
import { action } from 'mobx';

export default class MoveTreeStore extends BaseTreeState<PageModel> {

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
                parentNode.hasChildren = parentNode.children.length > 0;
            }
        }

        this.convertTree(this.origTreeInternal);
        this.selectedNode = null;
    }

    protected readonly contextMenuType: ContextMenuType = null;
    protected getTree = (): Promise<ApiResult<PageModel[]>> => null;
    protected getSubTree = (id: number): Promise<ApiResult<PageModel>> => null;

    protected mapElement(el: PageModel): ITreeElement {
        const treeElement = super.mapElement(el);
        treeElement.secondaryLabel = null;
        return treeElement;
    }

    private getIndex = (elements: PageModel[], id: number) => {
        for (let i = 0; i < elements.length; i += 1) {
            if (elements[i].id === id) {
                return i;
            }
        }
        return -1;
    }
}
