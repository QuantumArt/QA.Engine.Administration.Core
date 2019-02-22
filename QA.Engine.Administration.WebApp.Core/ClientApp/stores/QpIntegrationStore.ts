import DictionaryService from 'services/DictionaryService';
import QpAbstractItemFields from 'constants/QpAbstractItemFields';
import QpActionCodes from 'constants/QpActionCodes';
import QpCallbackProcNames from 'constants/QpCallbackProcNames';
import QpEntityCodes from 'constants/QpEntityCodes';
import { BackendEventObserver, executeBackendAction, ArticleFormState, ExecuteActionOptions, InitFieldValue } from 'qp/QP8BackendApi.Interaction';
import TreeStore from 'stores/TreeStore';
import SiteTreeStore from 'stores/TreeStore/SiteTreeStore';

export enum VersionType {
    Content = 'Content',
    Structural = 'Structural',
}

export default class QpIntegrationStore {
    constructor(treeStore: TreeStore) {
        this.treeStore = treeStore;
    }

    private qpContent: QpContentModel;
    private qpAbstractItem: string = 'QPAbstractItem';
    private readonly treeStore: TreeStore;

    private async fetchQPAbstractItemFields() {
        try {
            const response: ApiResult<QpContentModel> = await DictionaryService.getQpContent(this.qpAbstractItem);
            if (response.isSuccess) {
                this.qpContent = response.data;
            } else {
                throw response.error;
            }
        } catch (e) {
            console.error(e);
        }
    }

    private async updateSiteMapSubTree(id: number) {
        try {
            const tree = this.treeStore.resolveTreeStore();
            if (tree instanceof SiteTreeStore) {
                await tree.updateSubTree(id);
                const selectedNode = tree.selectedNode;
                [this.treeStore.getContentVersionsStore(), this.treeStore.getWidgetStore()]
                    .forEach(x => x.init(selectedNode));
            }
        } catch (e) {
            console.error(e);
        }
    }

    public async edit(id: number) {

        if (this.qpContent == null || this.qpContent.fields == null || this.qpContent.fields!.length === 0) {
            await this.fetchQPAbstractItemFields();
        }

        const func = (eventType: number, args: any) => this.updateCallback(eventType, args, id);
        new BackendEventObserver(QpCallbackProcNames.editCode, func);

        const executeOptions = QpIntegrationUtils.initOptions(QpCallbackProcNames.editCode, QpActionCodes.edit_article, id, this.qpContent.id);

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

    public async add(node: PageModel, versionType?: VersionType | null, name?: string, title?: string, discriminatorId?: number, extantionId?: number) {

        if (this.qpContent == null || this.qpContent.fields == null || this.qpContent.fields!.length === 0) {
            await this.fetchQPAbstractItemFields();
        }

        const func = (eventType: number, args: any) => this.addCallback(eventType, args, node.id);
        new BackendEventObserver(QpCallbackProcNames.addCode, func);

        const executeOptions = QpIntegrationUtils.initOptions(QpCallbackProcNames.addCode, QpActionCodes.new_article, 0, this.qpContent.id);

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
                    new FieldValueModel(null, discriminatorId, null, node.title, extantionId),
                    [
                        { fieldName: QpAbstractItemFields.versionOf, value: node.id },
                    ]);
                break;
            }
        }

        this.executeWindow(executeOptions);
    }

    public async history(id: number) {

        if (this.qpContent == null || this.qpContent.fields == null || this.qpContent.fields!.length === 0) {
            await this.fetchQPAbstractItemFields();
        }

        new BackendEventObserver(QpCallbackProcNames.historyCode, () => { });
        const executeOptions = QpIntegrationUtils.initOptions(QpCallbackProcNames.historyCode, QpActionCodes.list_article_version, id, this.qpContent.id);
        this.executeWindow(executeOptions);
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

    private updateCallback = (eventType: number, args: any, id: number) => {
        if (BackendEventObserver.EventType.SelectWindowClosed === eventType) {
            return;
        }

        if (BackendEventObserver.EventType.ActionExecuted === eventType) {
            if (args.actionCode === QpActionCodes.update_article || args.actionCode === QpActionCodes.update_article_and_up) {
                console.log('%cupdateCallback', 'color: magenta;', args);
                this.updateSiteMapSubTree(id);
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
                this.updateSiteMapSubTree(id);
                return;
            }  if (args.actionCode === QpActionCodes.update_article || args.actionCode === QpActionCodes.update_article_and_up) {
                // todo: update tree
                console.log('%caddCallback', 'color: magenta;', args);
                this.updateSiteMapSubTree(id);
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

    static initOptions = (procName: string, actionCode: string, entityId: number, contentId: number): ExecuteActionOptions => {
        const executeOptions = new ExecuteActionOptions();
        executeOptions.actionCode = actionCode;
        executeOptions.entityTypeCode = QpEntityCodes.article;
        executeOptions.entityId = entityId;
        executeOptions.parentEntityId = contentId;
        executeOptions.actionUID = QpIntegrationUtils.newGuid();
        executeOptions.callerCallback = procName;
        return executeOptions;
    }

    static getDefaultDisabledFields = (qpfields: QpFieldModel[], fields: string[] = []): string[] => {
        const result = [
            QpIntegrationUtils.getField(qpfields, QpAbstractItemFields.parent),
            QpIntegrationUtils.getField(qpfields, QpAbstractItemFields.discriminator),
            QpIntegrationUtils.getField(qpfields, QpAbstractItemFields.isPage),
            QpIntegrationUtils.getField(qpfields, QpAbstractItemFields.zoneName),
            QpIntegrationUtils.getField(qpfields, QpAbstractItemFields.extensionId),
            QpIntegrationUtils.getField(qpfields, QpAbstractItemFields.versionOf),
            QpIntegrationUtils.getField(qpfields, QpAbstractItemFields.indexOrder),
        ];
        fields.forEach(x => result.push(QpIntegrationUtils.getField(qpfields, x)));
        return result;
    }

    static getDefaultHideFields = (qpfields: QpFieldModel[], fields: string[] = []): string[] => {
        const result = [
            QpIntegrationUtils.getField(qpfields, QpAbstractItemFields.zoneName),
            QpIntegrationUtils.getField(qpfields, QpAbstractItemFields.indexOrder),
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
            { fieldName: QpIntegrationUtils.getField(qpfields, QpAbstractItemFields.isPage), value: true },
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
