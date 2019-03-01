import v4 from 'uuid/v4';
import SiteMapService from 'services/SiteMapService';
import { BaseTreeState } from 'stores/TreeStore/BaseTreeStore';
import OperationState from 'enums/OperationState';
import ContextMenuType from 'enums/ContextMenuType';
import TreeErrors from 'enums/TreeErrors';

export default class ArchiveTreeStore extends BaseTreeState<ArchiveModel> {

    protected contextMenuType: ContextMenuType = ContextMenuType.ARCHIVE;

    protected async getTree(): Promise<ApiResult<ArchiveModel[]>> {
        return await SiteMapService.getArchiveTree();
    }

    protected getSubTree(id: number): Promise<ApiResult<ArchiveModel>> {
        return SiteMapService.getArchiveSubTree(id);
    }
}
