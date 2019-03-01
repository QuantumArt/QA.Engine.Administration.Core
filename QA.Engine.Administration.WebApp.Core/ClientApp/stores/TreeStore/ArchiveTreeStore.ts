import SiteMapService from 'services/SiteMapService';
import { BaseTreeState } from 'stores/TreeStore/BaseTreeStore';
import ContextMenuType from 'enums/ContextMenuType';

export default class ArchiveTreeStore extends BaseTreeState<ArchiveModel> {

    protected contextMenuType: ContextMenuType = ContextMenuType.ARCHIVE;

    protected async getTree(): Promise<ApiResult<ArchiveModel[]>> {
        return await SiteMapService.getArchiveTree();
    }

    protected getSubTree(id: number): Promise<ApiResult<ArchiveModel>> {
        return SiteMapService.getArchiveSubTree(id);
    }
}
