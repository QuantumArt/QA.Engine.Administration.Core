import { action, observable, computed } from 'mobx';
import OperationState from 'enums/OperationState';
import SiteMapService from 'services/SiteMapService';

export class EditArticleState {

    @observable public title: string;
    @observable public state: OperationState = OperationState.NONE;
    @observable public fields: ExtensionFieldModel[] = [];
    @observable public isShowExtensionFields: boolean = false;
    public node: PageModel | ArchiveModel;
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
        this.title = node.title;
        this.isShowExtensionFields = false;
    }

    @action
    setTitle(title: string) {
        this.title = title;
    }

    @action
    showExtensionFields() {
        this.isShowExtensionFields = true;
    }

    @action
    public async fetchExtantionFields(): Promise<any> {
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
            console.error(e);
            this.state = OperationState.ERROR;
        }
    }
}

const editArticleStore = new EditArticleState();
export default editArticleStore;
