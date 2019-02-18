import { observable, action } from 'mobx';
import DictionaryService from 'services/DictionaryService';
import OperationState from 'enums/OperationState';
import PopupType from 'enums/PopupType';

export class PopupState {
    @observable state: OperationState = OperationState.NONE;
    @observable showPopup: boolean = false;
    @observable type: PopupType;

    discriminators: DiscriminatorModel[];
    itemId: number;
    title: string;

    @action
    public show(title: string = '') {
        this.getDiscriminators();
        this.title = title;
        this.showPopup = true;
    }

    @action
    public close() {
        this.showPopup = false;
    }

    public async getDiscriminators() {
        this.state = OperationState.PENDING;
        try {
            const response: ApiResult<DiscriminatorModel[]> = await DictionaryService.getDiscriminators();
            if (response.isSuccess) {
                this.discriminators = response.data;
                this.state = OperationState.SUCCESS;
            } else {
                throw response.error;
            }
        } catch (e) {
            this.state = OperationState.ERROR;
            console.error(e);
        }
    }
}

const popupStore = new PopupState();
export default popupStore;
