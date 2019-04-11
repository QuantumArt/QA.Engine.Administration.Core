import { observable, action, computed } from 'mobx';
import DictionaryService from 'services/DictionaryService';
import OperationState from 'enums/OperationState';
import ErrorHandler from 'stores/ErrorHandler';
import ErrorsTypes from 'constants/ErrorsTypes';

export default class RegionStore extends ErrorHandler {

    @observable public state: OperationState = OperationState.NONE;
    @observable public regions: RegionModel[];

    constructor() {
        super();
        this.fetchRegions();
    }

    @computed
    get useRegions(): boolean {
        return this.regions != null && this.regions.length > 0;
    }

    @action
    public async fetchRegions() {
        this.state = OperationState.PENDING;
        try {
            const response: ApiResult<RegionModel[]> = await DictionaryService.getFlatRegions();
            if (response.isSuccess) {
                this.regions = response.data
                    .sort((a, b) => a.title > b.title ? 1 : -1);
                this.state = OperationState.SUCCESS;
            } else {
                throw response.error;
            }
        } catch (e) {
            this.state = OperationState.ERROR;
            this.addError(ErrorsTypes.Regions.fetch, null, e);
        }
    }
}
