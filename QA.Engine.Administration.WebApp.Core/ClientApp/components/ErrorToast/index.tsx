import * as React from 'react';
import { inject, observer } from 'mobx-react';
import { Intent, Toast, Toaster } from '@blueprintjs/core';
import OperationState from 'enums/OperationState';
import TreeStore from 'stores/TreeStore';
import EditArticleStore from 'stores/EditArticleStore';
import { TreeErrors, PopupErrors } from 'enums/ErrorsTypes';
import TreeStore from 'stores/TreeStore';
import SiteTreeStore from 'stores/TreeStore/SiteTreeStore';
import ArchiveTreeStore from 'stores/TreeStore/ArchiveTreeStore';
import WidgetTreeStore from 'stores/TreeStore/WidgetTreeStore';
import ContentVersionTreeStore from 'stores/TreeStore/ContentVersionTreeStore';
import { ITreeErrorModel } from 'stores/TreeStore/BaseTreeStore';
import PopupStore from 'stores/PopupStore';

interface Props {
    treeStore?: TreeStore;
    editArticleStore?: EditArticleStore;
    popupStore?: PopupStore;
}

type CurrentTree = SiteTreeStore | ArchiveTreeStore | WidgetTreeStore | ContentVersionTreeStore;

@inject('treeStore', 'editArticleStore', 'popupStore')
@observer
export default class ErrorToast extends React.Component<Props> {

    private handleTreeErrorClick = (e: ITreeErrorModel, treeStore: TreeStore) => () => {
        switch (e.type) {
            case TreeErrors.fetch:
                treeStore.fetchTree();
                break;
            case TreeErrors.update:
                treeStore.updateSubTree(e.data);
                break;
            case TreeErrors.publish:
                treeStore.publish(e.data);
                break;
            case TreeErrors.archive:
                treeStore.archive(e.data);
                break;
            case TreeErrors.edit:
                treeStore.edit(e.data);
                break;
            case TreeErrors.restore:
                treeStore.restore(e.data);
                break;
            case TreeErrors.delete:
                treeStore.delete(e.data);
                break;
            case TreeErrors.reorder:
                treeStore.reorder(e.data);
                break;
            case TreeErrors.move:
                treeStore.move(e.data);
                break;
            default:
                break;
        }
    }

    private handlePopupErrorClick = (popupStore: PopupStore) => () => {
        popupStore.popupErrors.forEach((e) => {
            const { data: { itemId, type, title } } = e;
            popupStore.show(itemId, type, title);
        });
    }

    private handleDismiss = (i: number, cb: (i: number) => void) => () => {
        cb(i);
    }

    private renderToast = (
        e: IErrorModel<TreeErrors | PopupErrors>,
        i: number,
        currentStore: TreeStore | PopupStore,
        action: (t: TreeStore | PopupStore, ) => (e: React.MouseEvent<HTMLElement>) => void,
    ) => (
        <Toast
            message={`${e.type}. ${e.message}`}
            icon="warning-sign"
            intent={Intent.DANGER}
            action={{
                onClick: action(currentStore),
                icon: 'repeat',
            }}
            onDismiss={this.handleDismiss(i, currentStore.removeError)}
            key={e.id}
        />
    )

    render() {
        const { treeStore, popupStore } = this.props;

        return (
            <Toaster>
                {treeStore.state === OperationState.ERROR &&
                    treeStore.treeErrors.map((e, i) => this.renderToast(e, i, treeStore, this.handleTreeErrorClick))
                }
                {popupStore.state === OperationState.ERROR &&
                    popupStore.popupErrors.map(
                        (e, i) => this.renderToast(e, i, popupStore, this.handlePopupErrorClick),
                    )
                }
            </Toaster>
        );
    }
}
