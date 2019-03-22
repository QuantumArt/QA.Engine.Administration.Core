import * as React from 'react';
import { inject, observer } from 'mobx-react';
import { Intent, Toast, Toaster } from '@blueprintjs/core';
import OperationState from 'enums/OperationState';
import TreeStore from 'stores/TreeStore';
import EditArticleStore from 'stores/EditArticleStore';
import ErrorsTypes from 'constants/ErrorsTypes';
import ErrorHandler, { IErrorModel } from 'stores/ErrorHandler';
import PopupStore from 'stores/PopupStore';
import QpIntegrationStore from 'stores/QpIntegrationStore';
import TextStore from 'stores/TextStore';

interface Props {
    treeStore?: TreeStore;
    editArticleStore?: EditArticleStore;
    popupStore?: PopupStore;
    qpIntegrationStore?: QpIntegrationStore;
    textStore?: TextStore;
}

interface ToastOptions {
    error: IErrorModel;
    i: number;
    currentStore: ErrorHandler;
    action: (t: ErrorHandler, errorType: ErrorsTypes) => (e: React.MouseEvent<HTMLElement>) => void;
}

@inject('treeStore', 'editArticleStore', 'popupStore', 'qpIntegrationStore', 'textStore')
@observer
export default class ErrorToast extends React.Component<Props> {

    private handleTreeErrorClick = (store: TreeStore, error: IErrorModel) => () => {
        switch (error.type) {
            case ErrorsTypes.Tree.fetch:
                store.fetchTree();
                break;
            case ErrorsTypes.Tree.update:
                store.updateSubTree(error.data);
                break;
            case ErrorsTypes.Tree.publish:
                store.publish(error.data);
                break;
            case ErrorsTypes.Tree.archive:
                store.archive(error.data);
                break;
            case ErrorsTypes.Tree.edit:
                store.edit(error.data);
                break;
            case ErrorsTypes.Tree.restore:
                store.restore(error.data);
                break;
            case ErrorsTypes.Tree.delete:
                store.delete(error.data);
                break;
            case ErrorsTypes.Tree.reorder:
                store.reorder(error.data);
                break;
            case ErrorsTypes.Tree.move:
                store.move(error.data);
                break;
            default:
                break;
        }
    }

    private handlePopupErrorClick = (store: PopupStore, error: IErrorModel) => () => {
        const { data: { itemId, type, title, options } } = error;
        store.show(itemId, type, title, options);
    }

    private handleArticleError = (store: EditArticleStore) => () => {
        store.fetchExtensionFields();
    }

    private handleQPIntergrationError = (store: QpIntegrationStore, error: IErrorModel) => () => {
        switch (error.type) {
            case ErrorsTypes.QPintegration.fetchQpContentFields:
                store.fetchQpContentFields(error.data);
                break;
            case ErrorsTypes.QPintegration.fetchCustomActionCode:
                store.fetchCustomActionCode();
                break;
            case ErrorsTypes.QPintegration.fetchCultures:
                store.fetchCultures();
                break;
            case ErrorsTypes.QPintegration.fetchRegions:
                store.fetchRegions();
                break;
            case ErrorsTypes.QPintegration.updateSiteMapSubTree:
                store.updateSiteMapSubTree();
                break;
            default:
                break;
        }
    }

    private handleTextsError = (store: TextStore) => () => {
        store.fetchTexts();
    }

    private handleDismiss = (i: number, cb: (i: number) => void) => () => {
        cb(i);
    }

    private renderToast(options: ToastOptions) {
        const { error, i, currentStore, action } = options;
        return (
            <Toast
                message={`${error.type}. ${error.message}`}
                icon="warning-sign"
                intent={Intent.DANGER}
                action={{
                    onClick: action(currentStore, error),
                    icon: 'repeat',
                }}
                onDismiss={this.handleDismiss(i, currentStore.removeError)}
                key={error.id}
            />
        );
    }

    render() {
        const { treeStore, popupStore, editArticleStore, qpIntegrationStore, textStore } = this.props;

        return (
            <Toaster>
                {treeStore.state === OperationState.ERROR &&
                treeStore.errors.map((e, i) => this.renderToast({
                    i,
                    error: e,
                    currentStore: treeStore,
                    action: this.handleTreeErrorClick,
                }))}
                {popupStore.state === OperationState.ERROR &&
                popupStore.errors.map((e, i) => this.renderToast({
                    i,
                    error: e,
                    currentStore: popupStore,
                    action: this.handlePopupErrorClick,
                }))}
                {editArticleStore.state === OperationState.ERROR &&
                editArticleStore.errors.map((e, i) => this.renderToast({
                    i,
                    error: e,
                    currentStore: editArticleStore,
                    action: this.handleArticleError,
                }))}
                {qpIntegrationStore.state === OperationState.ERROR &&
                qpIntegrationStore.errors.map((e, i) => this.renderToast({
                    i,
                    error: e,
                    currentStore: qpIntegrationStore,
                    action: this.handleQPIntergrationError,
                }))}
                {textStore.state === OperationState.ERROR &&
                textStore.errors.map((e, i) => this.renderToast({
                    i,
                    error: e,
                    currentStore: textStore,
                    action: this.handleTextsError,
                }))}
            </Toaster>
        );
    }
}
