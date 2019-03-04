import { action, observable, computed } from 'mobx';
import OperationState from 'enums/OperationState';
import SiteMapService from 'services/SiteMapService';
import ErrorHandler from 'stores/ErrorHandler';
import ErrorsTypes from 'constants/ErrorsTypes';

export default class EditArticleStore extends ErrorHandler {

    @observable public title: string;
    @observable public isVisible: boolean;
    @observable public isInSiteMap: boolean;

    @observable public state: OperationState = OperationState.NONE;
    @observable public fields: ExtensionFieldModel[] = [];
    @observable public isShowExtensionFields: boolean = false;
    public node: PageModel | ArchiveModel;
    public isEditable: boolean;
    private extensionFieldsJson: string = JSON.stringify([]);

    @computed
    get changedFields(): ExtensionFieldModel[] {
        const orig: ExtensionFieldModel[] = JSON.parse(this.extensionFieldsJson);
        return this.fields.filter(x =>
            orig.filter(y =>
                y.fieldName === x.fieldName && y.value !== x.value).length > 0);
    }

    @action
    init(node: PageModel | ArchiveModel) {
        this.node = node;
        if (node == null) {
            this.title = this.isVisible = this.isInSiteMap = this.isShowExtensionFields = this.isEditable = null;
            return;
        }
        this.title = node.title;
        this.isVisible = node.isVisible;
        this.isInSiteMap = node.isInSiteMap;
        this.isShowExtensionFields = false;
        this.isEditable = !node.isArchive;
    }

    @action
    setTitle(title: string) {
        this.title = title;
    }
    @action
    setIsVisible(isVisible: boolean) {
        this.isVisible = isVisible;
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
                this.fields = response.data;
                this.extensionFieldsJson = JSON.stringify(this.fields.length > 0 ? this.fields : []);
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
