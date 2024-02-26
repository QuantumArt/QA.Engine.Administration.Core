import { observable } from 'mobx';
import DictionaryService from 'services/DictionaryService';
import QpAbstractItemFields from 'constants/QpAbstractItemFields';
import QpActionCodes from 'constants/QpActionCodes';
import QpCallbackProcNames from 'constants/QpCallbackProcNames';
import QpEntityCodes from 'constants/QpEntityCodes';
// import { BackendEventObserver, executeBackendAction, ArticleFormState, ExecuteActionOptions, InitFieldValue, OpenSelectWindowOptions, openSelectWindow, EntitiesSelectedArgs } from 'qp/QP8BackendApi.Interaction';
import {
    BackendEventObserver,
    executeBackendAction,
    ArticleFormState,
    ExecuteActionOptions,
    InitFieldValue,
    OpenSelectWindowOptions,
    openSelectWindow,
    EntitiesSelectedArgs,
} from 'qp8backendapi-interaction';
import TreeStore from 'stores/TreeStore';
import ErrorHandler from 'stores/ErrorHandler';
import ErrorsTypes from 'constants/ErrorsTypes';
import OperationState from 'enums/OperationState';

export enum VersionType {
    Content = 'Content',
    Structural = 'Structural',
}

export default class QpIntegrationStore extends ErrorHandler {
    constructor(treeStore: TreeStore) {
        super();
        this.treeStore = treeStore;
    }

    @observable state: OperationState = OperationState.NONE;

    async fetchQpContentFields(qpContentName: string) {
        this.state = OperationState.PENDING;
        try {
            const response: ApiResult<QpContentModel> = await DictionaryService.getQpContent(qpContentName);
            if (response.isSuccess) {
                this.qpContent = response.data;
                this.state = OperationState.SUCCESS;
            } else {
                throw response.error;
            }
        } catch (e) {
            this.state = OperationState.ERROR;
            this.addError(ErrorsTypes.QPintegration.fetchQpContentFields, qpContentName, e);
        }
    }

    async fetchCultures() {
        this.state = OperationState.PENDING;
        try {
            const response: ApiResult<CultureModel[]> = await DictionaryService.getCultures();
            if (response.isSuccess) {
                this.cultures = response.data;
                this.state = OperationState.SUCCESS;
            } else {
                throw response.error;
            }
        } catch (e) {
            this.state = OperationState.ERROR;
            this.addError(ErrorsTypes.QPintegration.fetchCultures, null, e);
        }
    }

    async fetchRegions() {
        this.state = OperationState.PENDING;
        try {
            const response: ApiResult<RegionModel[]> = await DictionaryService.getFlatRegions();
            if (response.isSuccess) {
                this.regions = response.data;
                this.state = OperationState.SUCCESS;
            } else {
                throw response.error;
            }
        } catch (e) {
            this.state = OperationState.ERROR;
            this.addError(ErrorsTypes.QPintegration.fetchRegions, null, e);
        }
    }

    async fetchCustomActionCode(alias: string) {
        this.state = OperationState.PENDING;
        try {
            const response: ApiResult<CustomActionModel> = await DictionaryService.getCustomAction(alias);
            if (response.isSuccess) {
                this.customAction = response.data;
                if (this.customAction == null) {
                    this.state = OperationState.ERROR;
                    this.addError(ErrorsTypes.QPintegration.fetchCustomActionCodeNotFound, null, '');
                } else {
                    this.state = OperationState.SUCCESS;
                }
            } else {
                throw response.error;
            }
        } catch (e) {
            this.state = OperationState.ERROR;
            this.addError(ErrorsTypes.QPintegration.fetchCustomActionCode, alias, e);
        }
    }

    public updateSiteMapSubTree() {
        try {
            this.treeStore.updateSubTree();
        } catch (e) {
            this.addError(ErrorsTypes.QPintegration.updateSiteMapSubTree, null, e);
        }
    }

    private qpAbstractItem: string = 'QPAbstractItem';
    private qpCulture: string = 'QPCulture';
    private qpRegion: string = 'QPRegion';

    private qpContent: QpContentModel;
    private cultures: CultureModel[];
    private regions: RegionModel[];
    private customAction: CustomActionModel;
    private readonly treeStore: TreeStore;

    public async edit(id: number) {

        await this.fetchQpContentFields(this.qpAbstractItem);

        const func = (eventType: number, args: any) => this.updateCallback(eventType, args, id);
        new BackendEventObserver(QpCallbackProcNames.editCode, func);

        const executeOptions = QpIntegrationUtils.initOptions(QpCallbackProcNames.editCode, QpActionCodes.edit_article, QpEntityCodes.article, id, this.qpContent.id);

        if (executeOptions.options == null) {
            executeOptions.options = new ArticleFormState();
        }

        executeOptions.options.disabledFields = QpIntegrationUtils.getDefaultDisabledFields(this.qpContent.fields, [
            QpAbstractItemFields.contentName,
            'Data.Schedule.ScheduleType',
        ]);
        executeOptions.options.hideFields = QpIntegrationUtils.getDefaultHideFields(this.qpContent.fields, [
            QpAbstractItemFields.versionOf,
        ]);
        executeOptions.options.disabledActionCodes = QpIntegrationUtils.getDefaultDisabledActionCodes();

        this.executeWindow(executeOptions);
    }

    public async add<T extends {
        id: number,
        parentId?: null | number,
        alias: string,
        title:string }>(node: T, versionType?: VersionType | null, name?: string, title?: string, discriminatorId?: number, extantionId?: number) {

        await this.fetchQpContentFields(this.qpAbstractItem);

        const func = (eventType: number, args: any) => this.addCallback(eventType, args, node.id);
        new BackendEventObserver(QpCallbackProcNames.addCode, func);

        const executeOptions = QpIntegrationUtils.initOptions(QpCallbackProcNames.addCode, QpActionCodes.new_article, QpEntityCodes.article, 0, this.qpContent.id);

        if (executeOptions.options == null) {
            executeOptions.options = new ArticleFormState();
        }

        executeOptions.options.disabledFields = QpIntegrationUtils.getDefaultDisabledFields(this.qpContent.fields, [
            QpAbstractItemFields.contentName,
            'Data.Schedule.ScheduleType',
        ]);
        executeOptions.options.hideFields = QpIntegrationUtils.getDefaultHideFields(this.qpContent.fields, [
            QpAbstractItemFields.versionOf,
        ]);
        executeOptions.options.disabledActionCodes = QpIntegrationUtils.getDefaultDisabledActionCodes([
            QpActionCodes.list_article_version,
        ]);

        if (versionType == null) {
            const model = new FieldValueModel(node.id, discriminatorId, name, title, extantionId);
            executeOptions.options.initFieldValues = QpIntegrationUtils.getFieldValues(this.qpContent.fields, model);
        } else {
            switch (versionType) {
                case VersionType.Structural:
                    executeOptions.options.disabledFields = QpIntegrationUtils.getDefaultDisabledFields(this.qpContent.fields, [
                        QpAbstractItemFields.contentName,
                    ]);
                    executeOptions.options.initFieldValues = QpIntegrationUtils.getFieldValues(
                    this.qpContent.fields,
                    new FieldValueModel(node.parentId, discriminatorId, node.alias, node.title, extantionId));
                    break;
                case VersionType.Content:
                    executeOptions.options.hideFields = QpIntegrationUtils.getDefaultHideFields(this.qpContent.fields);
                    executeOptions.options.initFieldValues = QpIntegrationUtils.getFieldValues(
                    this.qpContent.fields,
                    new FieldValueModel(null, discriminatorId, null, node.title, extantionId), [
                        { fieldName: QpAbstractItemFields.versionOf, value: node.id },
                    ]);
                    break;
            }
        }

        this.executeWindow(executeOptions);
    }

    public async history(id: number) {

        await this.fetchQpContentFields(this.qpAbstractItem);

        new BackendEventObserver(QpCallbackProcNames.historyCode, () => { });
        const executeOptions = QpIntegrationUtils.initOptions(QpCallbackProcNames.historyCode, QpActionCodes.list_article_version, QpEntityCodes.article, id, this.qpContent.id);
        this.executeWindow(executeOptions);
    }

    public link(extensionId: number, relatedId: number) {
        new BackendEventObserver(QpCallbackProcNames.editCode, () => { });
        const executeOptions = QpIntegrationUtils.initOptions(QpCallbackProcNames.editCode, QpActionCodes.edit_article, QpEntityCodes.article, relatedId, extensionId);
        this.executeWindow(executeOptions);
    }

    public async preview(id: number, alias: string) {

        await this.showCultureSelector(id, alias);
    }

    private async showCultureSelector(id: number, alias: string) {

        await this.fetchQpContentFields(this.qpCulture);
        await this.fetchCultures();

        if (!this.cultures || this.cultures.length <= 1) {
            this.showCustomAction(id, alias, null, null);
            return;
        }

        const func = (eventType: number, args: EntitiesSelectedArgs) => {
            if (BackendEventObserver.EventType.SelectWindowClosed === eventType) {
                return;
            }
            if (BackendEventObserver.EventType.EntitiesSelected === eventType) {
                this.showRegionSelector(id, alias, args.selectedEntityIDs);
            }
        };

        new BackendEventObserver(QpCallbackProcNames.addCode, func);
        const executeOptions = QpIntegrationUtils.initSelectOptions(false, this.qpContent.id, this.cultures.map(x => x.id), QpCallbackProcNames.addCode);
        if (executeOptions.options == null) {
            executeOptions.options = new ArticleFormState();
        }
        executeOptions.options.filter = `c.Archive = 0 and c.content_item_id in (${this.cultures.map(x => x.id).join(',')})`;
        this.executeSelectWindow(executeOptions);
    }

    private async showRegionSelector(id: number, alias: string, cultureIds: number[]) {

        await this.fetchQpContentFields(this.qpRegion);
        await this.fetchRegions();

        if (!this.regions || this.regions.length <= 1) {
            this.showCustomAction(id, alias, cultureIds, null);
            return;
        }

        const func = (eventType: number, args: EntitiesSelectedArgs) => {
            if (BackendEventObserver.EventType.SelectWindowClosed === eventType) {
                return;
            }
            if (BackendEventObserver.EventType.EntitiesSelected === eventType) {
                this.showCustomAction(id, alias, cultureIds, args.selectedEntityIDs);
            }
        };
        new BackendEventObserver(QpCallbackProcNames.addCode, func);
        const executeOptions = QpIntegrationUtils.initSelectOptions(false, this.qpContent.id, this.regions.map(x => x.id), QpCallbackProcNames.addCode);
        this.executeSelectWindow(executeOptions);
    }

    private async showCustomAction(id: number, alias: string, cultureIds: number[], regionIds: number[]) {

        await this.fetchCustomActionCode(alias);

        const procName = QpCallbackProcNames.previewCode;
        const entityTypeCode = QpEntityCodes.site;
        const entityId = QpIntegrationUtils.headerData.SiteId;

        if (this.customAction == null) {
            return;
        }

        new BackendEventObserver(procName, () => {});
        const executeOptions = QpIntegrationUtils.initOptions(procName, this.customAction.code, entityTypeCode, entityId, this.customAction.id);

        if (executeOptions.options == null) {
            executeOptions.options = new ArticleFormState();
        }
        executeOptions.options.additionalParams = {};
        if (this.customAction.itemIdParamName != null) {
            executeOptions.options.additionalParams[this.customAction.itemIdParamName] = id;
        }
        if (cultureIds != null && this.customAction.cultureParamName != null) {
            executeOptions.options.additionalParams[this.customAction.cultureParamName] = this.cultures.find(x => x.id === cultureIds[0]).name;
        }
        if (regionIds != null && this.customAction.regionParamName != null) {
            executeOptions.options.additionalParams[this.customAction.regionParamName] = regionIds[0];
        }
        this.executeTab(executeOptions);
    }

    private executeWindow = (executeOptions: ExecuteActionOptions) => {
        executeOptions.isWindow = true;
        executeOptions.changeCurrentTab = false;

        if (window.parent == null) {
            alert('Функционал недоступен.');
            return;
        }

        executeBackendAction(executeOptions, QpIntegrationUtils.hostUID(), window.parent);
    }

    private executeSelectWindow = (executeOptions: OpenSelectWindowOptions) => {
        if (window.parent == null) {
            alert('Функционал недоступен.');
            return;
        }

        openSelectWindow(executeOptions, QpIntegrationUtils.hostUID(), window.parent);
    }

    private executeTab = (executeOptions: ExecuteActionOptions) => {
        executeOptions.isWindow = false;
        executeOptions.changeCurrentTab = false;

        if (window.parent == null) {
            alert('Функционал недоступен.');
            return;
        }

        executeBackendAction(executeOptions, QpIntegrationUtils.hostUID(), window.parent);
    }

    private updateCallback = (eventType: number, args: any, id: number) => {
        if (BackendEventObserver.EventType.SelectWindowClosed === eventType) {
            return;
        }

        if (BackendEventObserver.EventType.ActionExecuted === eventType) {
            if (args.actionCode === QpActionCodes.update_article || args.actionCode === QpActionCodes.update_article_and_up) {
                console.log('%cupdateCallback', 'color: magenta;', args);
                this.updateSiteMapSubTree();
                return;
            }
        }
    }

    private addCallback = (eventType: number, args: any, id: number) => {
        if (BackendEventObserver.EventType.HostUnbinded === eventType) {
            return;
        }

        if (BackendEventObserver.EventType.ActionExecuted === eventType) {
            if (args.actionCode === QpActionCodes.save_article || args.actionCode === QpActionCodes.save_article_and_up) {
                // todo: update tree
                console.log('%caddCallback', 'color: magenta;', args);
                this.updateSiteMapSubTree();
                return;
            }  if (args.actionCode === QpActionCodes.update_article || args.actionCode === QpActionCodes.update_article_and_up) {
                // todo: update tree
                console.log('%caddCallback', 'color: magenta;', args);
                this.updateSiteMapSubTree();
                return;
            }
        }
    }
}

class QpIntegrationUtils {
    static newGuid = () => {
        const s4 = QpIntegrationUtils.s4;
        return `${s4()}${s4()}-${s4()}-${s4()}-${s4()}-${s4()}${s4()}${s4()}`;
    }

    static hostUID = () =>
        (window.location.search.substring(1).split('&').filter(x => x.indexOf('hostUID=') > -1)[0]!.split('=')[1])

    static initOptions = (procName: string, actionCode: string, entityTypeCode: string, entityId: number, contentId: number): ExecuteActionOptions => {
        const executeOptions = new ExecuteActionOptions();
        executeOptions.actionCode = actionCode;
        executeOptions.entityTypeCode = entityTypeCode; // QpEntityCodes.article;
        executeOptions.entityId = entityId;
        executeOptions.parentEntityId = contentId;
        executeOptions.actionUID = QpIntegrationUtils.newGuid();
        executeOptions.callerCallback = procName;
        return executeOptions;
    }

    static initSelectOptions = (isMultiple: boolean, entityId: number, selectedEntityIDs: number[], callback: string): OpenSelectWindowOptions => {
        const executeOptions = new OpenSelectWindowOptions();
        executeOptions.selectActionCode = isMultiple ? QpActionCodes.multiple_select_article : QpActionCodes.select_article;
        executeOptions.entityTypeCode = QpEntityCodes.article;
        executeOptions.isMultiple = isMultiple,
        executeOptions.parentEntityId = entityId;
        executeOptions.selectWindowUID = QpIntegrationUtils.newGuid();
        executeOptions.callerCallback = callback;
        executeOptions.selectedEntityIDs = selectedEntityIDs;
        return executeOptions;
    }

    static getDefaultDisabledFields = (qpfields: QpFieldModel[], fields: string[] = []): string[] => {
        const result = [
            QpIntegrationUtils.getField(qpfields, QpAbstractItemFields.parent),
            QpIntegrationUtils.getField(qpfields, QpAbstractItemFields.discriminator),
            QpIntegrationUtils.getField(qpfields, QpAbstractItemFields.extensionId),
            QpIntegrationUtils.getField(qpfields, QpAbstractItemFields.versionOf),
        ];
        fields.forEach(x => result.push(QpIntegrationUtils.getField(qpfields, x)));
        return result;
    }

    static getDefaultHideFields = (qpfields: QpFieldModel[], fields: string[] = []): string[] => {
        const result = [
            QpIntegrationUtils.getField(qpfields, QpAbstractItemFields.isPage),
        ];
        fields.forEach(x => result.push(QpIntegrationUtils.getField(qpfields, x)));
        return result;
    }

    static getDefaultDisabledActionCodes = (fields: string[] = []): string[] => {
        const result = [
            QpActionCodes.move_to_archive_article,
            QpActionCodes.remove_article,
        ];
        fields.forEach(x => result.push(x));
        return result;
    }

    static getFieldValues = (qpfields: QpFieldModel[], model: FieldValueModel, fieldValues: InitFieldValue[] = []): InitFieldValue[] => {
        const result = [
            { fieldName: QpIntegrationUtils.getField(qpfields, QpAbstractItemFields.parent), value: model.parentId },
            { fieldName: QpIntegrationUtils.getField(qpfields, QpAbstractItemFields.discriminator), value: model.discriminatorId === 0 ? null : model.discriminatorId },
            { fieldName: QpIntegrationUtils.getField(qpfields, QpAbstractItemFields.contentName), value: model.name },
            { fieldName: QpIntegrationUtils.getField(qpfields, QpAbstractItemFields.title), value: model.title },
            { fieldName: 'Data.Schedule.ScheduleType', value: 'Visible' },
            { fieldName: QpIntegrationUtils.getField(qpfields, QpAbstractItemFields.extensionId), value: model.extantionId === 0 ? null : model. extantionId },
        ];
        fieldValues.forEach(x => result.push({ fieldName: QpIntegrationUtils.getField(qpfields, x.fieldName), value: x.value }));
        return result;
    }

    private static getField = (fields: QpFieldModel[], fieldName: string) => {
        const field = fields.filter(x => x.name === fieldName)[0];
        return field == null ? fieldName : field.fieldId;
    }

    private static s4 = () =>
        (Math.floor(Math.random() * 0x10000).toString(16))

    static get headerData(): any {
        const getQueryVariable = (variable: string) => {
            const result = window.location.search.substring(1).split('&')
                .map(x => ({ name: x.split('=')[0], value: x.split('=')[1] }))
                .filter(x => x.name === variable)[0];
            return result == null ? null : result.value;
        };

        return {
            BackendSid: getQueryVariable('backend_sid'),
            CustomerCode: getQueryVariable('customerCode'),
            HostId: getQueryVariable('hostUID'),
            SiteId: getQueryVariable('site_id'),
        };
    }

}

class FieldValueModel {
    constructor(parentId?: number, discriminatorId?: number, name?: string, title?: string, extantionId?: number) {
        this.parentId = parentId;
        this.discriminatorId = discriminatorId;
        this.name = name;
        this.title = title;
        this.extantionId = extantionId;
    }
    parentId?: number;
    discriminatorId?: number;
    name?: string;
    title?: string;
    extantionId?: number;
}
