import { observable, action } from 'mobx';
import DictionaryService from 'services/DictionaryService';
import OperationState from 'enums/OperationState';
import PopupType from 'enums/PopupType';
import SiteMapService from 'services/SiteMapService';
import ErrorsTypes from 'constants/ErrorsTypes';
import ErrorHandler from 'stores/ErrorHandler';

export default class PopupStore extends ErrorHandler {
    @observable state: OperationState = OperationState.NONE;
    @observable type: PopupType;
    @observable showPopup: boolean = false;

    discriminators: DiscriminatorModel[];
    contentVersions: PageModel[];
    itemId: number;
    title: string;
    options: any;

    @action
    public async show(itemId: number, type: PopupType, title: string, options?: any) {
        this.state = OperationState.NONE;

        const useDiscriminators = [PopupType.ADD, PopupType.ADDVERSION, PopupType.ADDWIDGET];
        const useContentVersions = [PopupType.ARCHIVE];

        if (useDiscriminators.indexOf(type) > -1) {
            this.getDiscriminators(itemId, type, title, options);
        }

        if (useContentVersions.indexOf(type) > -1) {
            this.getContentVersions(itemId, type, title, options);
        }
        this.itemId = itemId;
        this.type = type;
        this.title = title;
        this.showPopup = true;
        this.options = options;
    }

    @action
    public close() {
        this.showPopup = false;
    }

    private async getDiscriminators(itemId: number, type: PopupType, title: string, options?: any) {
        const isPage = type !== PopupType.ADDWIDGET;
        this.state = OperationState.PENDING;
        try {
            const response: ApiResult<DiscriminatorModel[]> = await DictionaryService.getDiscriminators();
            if (response.isSuccess) {
                this.discriminators = response.data.filter(x => x.isPage === isPage);
                this.state = OperationState.SUCCESS;
            } else {
                throw response.error;
            }
        } catch (e) {
            this.state = OperationState.ERROR;
            this.addError(
                ErrorsTypes.Popup.discriminators,
                { itemId, type, title, options },
                e,
            );
        }
    }

    private async getContentVersions(itemId: number, type: PopupType, title: string, options?: any) {
        this.state = OperationState.PENDING;
        try {
            const response: ApiResult<PageModel> = await SiteMapService.getSiteMapSubTree(itemId);
            if (response.isSuccess) {
                this.contentVersions = response.data == null ? [] : response.data.contentVersions;
                this.state = OperationState.SUCCESS;
            } else {
                throw response.error;
            }
        } catch (e) {
            this.state = OperationState.ERROR;
            this.addError(
                ErrorsTypes.Popup.versions,
                { itemId, type, title, options },
                e,
            );
        }
    }
}
