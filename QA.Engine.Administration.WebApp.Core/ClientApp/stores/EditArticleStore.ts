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
    public relatedItems: Map<number, string>;
    public relatedManyToOneItems: Map<number, Map<number, string>>;
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
        this.isEditable = !node.archive && node.extensionId != null;
        this.isShowExtensionFields = false;
        this.relatedItems = new Map<number, string>();
        this.relatedManyToOneItems = new Map<number, Map<number, string>>();
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
    async fetchExtensionFields(): Promise<void> {
        this.state = OperationState.PENDING;
        const id = this.node.id;
        const extensionId = this.node.extensionId;
        try {
            const response: ApiResult<ExtensionFieldModel[]> = await SiteMapService.getExtensionFields(id, extensionId);
            if (response.isSuccess) {
                const fields = response.data || [];

                const itemNames = <any[]>[];
                const relationFields = fields.filter(x => x.typeName.toLowerCase() === 'relation' && x.value);
                relationFields.forEach((x) => {
                    itemNames.push(this.fetchRelatedItemName(x.value, x.attributeId));
                });
                const manyToOneRelationFields = fields.filter(x => x.typeName.toLowerCase() === 'relation many-to-one' && x.value);
                manyToOneRelationFields.forEach((x) => {
                    itemNames.push(this.fetchManyToOneRelatedItemName(x.value, this.node.id, x.attributeId));
                });
                await Promise.all(itemNames);

                this.fields = fields;
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
    removeError = (i: number) => {
        this.state = OperationState.NONE;
        this.errors.splice(i, 1);
        this.init(this.node);
    }

    private async fetchRelatedItemName(id: number, attributeId: number) {
        const response: ApiResult<string> = await SiteMapService.getRelatedItemName(id, attributeId);
        if (response.isSuccess) {
            this.relatedItems.set(attributeId, response.data);
        } else {
            throw response.error;
        }
    }

    private async fetchManyToOneRelatedItemName(id: number, value: number, attributeId: number) {
        const response: ApiResult<{ [key: number]: string; }> = await SiteMapService.getManyToOneRelatedItemNames(id, value, attributeId);
        if (response.isSuccess) {
            const data = response.data;
            const map = new Map<number, string>();
            for (const field in data) {
                map.set(+field, data[field]);
            }
            if (map.size > 0) {
                this.relatedManyToOneItems.set(attributeId, map);
            }
        } else {
            throw response.error;
        }
    }
}
