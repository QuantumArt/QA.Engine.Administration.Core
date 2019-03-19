import { action, observable } from 'mobx';
import OperationState from 'enums/OperationState';
import SiteMapService from 'services/SiteMapService';
import ErrorHandler from 'stores/ErrorHandler';
import ErrorsTypes from 'constants/ErrorsTypes';

export default class EditArticleStore extends ErrorHandler {

    @observable public title: string;
    @observable public isInSiteMap: boolean;

    @observable public state: OperationState = OperationState.NONE;
    @observable public fields: ExtensionFieldModel[] = [];
    @observable public isShowExtensionFields: boolean = false;
    public isEditable: boolean;
    private node: PageModel | ArchiveModel;

    @action
    init(node: PageModel | ArchiveModel) {
        this.node = node;
        if (node == null) {
            this.title = this.isInSiteMap = this.isEditable = this.isShowExtensionFields = null;
            return;
        }
        this.title = node.title;
        this.isInSiteMap = node.isInSiteMap;
        this.isEditable = !node.isArchive;
        this.isShowExtensionFields = false;
    }

    @action
    setTitle(title: string) {
        this.title = title;
    }
    @action
    setIsInSiteMap(isInSiteMap: boolean) {
        this.isInSiteMap = isInSiteMap;
    }
    @action
    showExtensionFields() {
        this.isShowExtensionFields = true;
    }

    @action
    public async fetchExtentionFields(): Promise<void> {
        this.state = OperationState.PENDING;
        const id = this.node.id;
        const extantionId = this.node.extensionId;
        try {
            const response: ApiResult<ExtensionFieldModel[]> = await SiteMapService.getExtantionFields(id, extantionId);
            if (response.isSuccess) {
                this.fields = response.data || [];
                this.state = OperationState.SUCCESS;
            } else {
                throw response.error;
            }
        } catch (e) {
            this.state = OperationState.ERROR;
            this.addError(ErrorsTypes.ExtensionFields.fetch, { node: this.node }, e);
        }
    }

    @action
    public removeError = (i: number) => {
        this.state = OperationState.NONE;
        this.errors.splice(i, 1);
        this.init(this.node);
    }
}
