import { observable, action } from 'mobx';
import DictionaryService from 'services/DictionaryService';
import OperationState from 'enums/OperationState';
import ErrorHandler from 'stores/ErrorHandler';
import ErrorsTypes from 'constants/ErrorsTypes';

export default class TextStore extends ErrorHandler {

    @observable public state: OperationState = OperationState.NONE;
    @observable.ref public texts: { [key: string]: string; } = {};

    constructor() {
        super();
        this.fetchTexts();
    }

    @action
    public async fetchTexts() {
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
            this.state = OperationState.ERROR;
            this.addError(ErrorsTypes.Texts.fetch, null, e);
        }
    }
}
