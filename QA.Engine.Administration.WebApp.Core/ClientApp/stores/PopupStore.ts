import { observable, action } from 'mobx';
import DictionaryService from 'services/DictionaryService';
import OperationState from 'enums/OperationState';
import PopupType from 'enums/PopupType';
import SiteMapService from 'services/SiteMapService';

export default class PopupStore {
    @observable state: OperationState = OperationState.NONE;
    @observable showPopup: boolean = false;
    @observable type: PopupType;

    discriminators: DiscriminatorModel[];
    contentVersions: PageModel[];
    itemId: number;
    title: string;

    @action
    public show(itemId: number, type: PopupType, title: string) {
        this.state = OperationState.NONE;
        const useDiscriminators = [PopupType.ADD, PopupType.ADDVERSION];
        if (useDiscriminators.indexOf(type) > -1) {
            this.getDiscriminators();
        }
        if (type === PopupType.ARCHIVE && itemId != null) {
            this.getContentVersions(itemId);
        }
        this.itemId = itemId;
        this.type = type;
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

    public async getContentVersions(itemId: number) {
        this.state = OperationState.PENDING;
        try {
            const response: ApiResult<PageModel> = await SiteMapService.getSiteMapSubTree(itemId);
            if (response.isSuccess) {
                this.contentVersions = response.data.contentVersions;
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
