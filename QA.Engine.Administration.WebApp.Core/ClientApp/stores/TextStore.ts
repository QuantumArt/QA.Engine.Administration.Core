import DictionaryService from 'services/DictionaryService';
import OperationState from 'enums/OperationState';
import { observable, action } from 'mobx';

export default class TextStore {

    @observable public state: OperationState = OperationState.NONE;
    @observable public texts: { [key: string]: string; } = {};

    constructor() {
        this.fetchTexts();
    }

    @action
    private async fetchTexts() {
        this.state = OperationState.PENDING;
        try {
            const response: ApiResult<{ [key: string]: string; }> = await DictionaryService.getTexts();
            if (response.isSuccess) {
                this.texts = response.data;
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
