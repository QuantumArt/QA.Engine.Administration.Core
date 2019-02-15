import SiteMapService from 'services/SiteMapService';
import { observable, action } from 'mobx';
import OperationState from 'enums/OperationState';

export class ExtensionFieldsState {
    @observable public state: OperationState = OperationState.NONE;
    public fields: ExtensionFieldModel[];
    public changedFields: ExtensionFieldModel[];

    @action
    public async fetchExtantionFields(id: number, extantionId: number): Promise<any> {
        this.state = OperationState.PENDING;
        try {
            const response: ApiResult<ExtensionFieldModel[]> = await SiteMapService.getExtantionFields(id, extantionId);
            if (response.isSuccess) {
                this.fields = response.data;
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

const extantionFieldsStore = new ExtensionFieldsState();
export default extantionFieldsStore;
