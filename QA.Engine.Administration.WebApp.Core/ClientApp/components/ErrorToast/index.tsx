import * as React from 'react';
import { inject, observer } from 'mobx-react';
import { Intent, Toast, Toaster } from '@blueprintjs/core';
import OperationState from 'enums/OperationState';
import TreeStore from 'stores/TreeStore';
import EditArticleStore from 'stores/EditArticleStore';
import TreeErrors from 'enums/TreeErrors';
import { ITreeErrorModel } from 'stores/TreeStore/BaseTreeStore';
import SiteTreeStore from 'stores/TreeStore/SiteTreeStore';
import ArchiveTreeStore from 'stores/TreeStore/ArchiveTreeStore';
import WidgetTreeStore from 'stores/TreeStore/WidgetTreeStore';
import ContentVersionTreeStore from 'stores/TreeStore/ContentVersionTreeStore';
import TreeStoreType from 'enums/TreeStoreType';

interface Props {
    treeStore?: TreeStore;
    editArticleStore?: EditArticleStore;
}

type CurrentTree = SiteTreeStore | ArchiveTreeStore | WidgetTreeStore | ContentVersionTreeStore;

@inject('treeStore', 'editArticleStore')
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

    private handleDismiss = (i: number, cb: (i: number) => void) => () => {
        cb(i);
    }

    // private renderToast = (e:ITreeErrorModel, i: number, currentTree: CurrentTree) => (
    private renderToast = (e: ITreeErrorModel, i: number, treeStore: TreeStore) => (
        <Toast
            message={`${e.type}. ${e.message}`}
            icon="warning-sign"
            intent={Intent.DANGER}
            action={{
                // onClick: this.handleTreeErrorClick(currentTree),
                onClick: this.handleTreeErrorClick(e, treeStore),
                icon: 'repeat',
            }}
            // onDismiss={this.handleDismiss(i, currentTree.removeError)}
            onDismiss={this.handleDismiss(i, treeStore.removeError)}
            key={e.id}
        />
    )

    render() {
        const { treeStore } = this.props;
        // const siteTree = treeStore.resolveTreeStore();
        // const widgetTree = treeStore.getWidgetStore();
        // const contentVersionsStore = treeStore.getContentVersionsStore();

        return (
            <Toaster>
                {/* {siteTree.treeState === OperationState.ERROR &&
                    siteTree.treeErrors.map((e, i) => this.renderToast(e, i, siteTree))
                }
                {widgetTree.treeState === OperationState.ERROR &&
                    widgetTree.treeErrors.map((e, i) => this.renderToast(e, i, widgetTree))
                }
                {contentVersionsStore.treeState === OperationState.ERROR &&
                    contentVersionsStore.treeErrors.map((e, i) => this.renderToast(e, i, contentVersionsStore))
                } */}
                {treeStore.state === OperationState.ERROR &&
                    treeStore.treeErrors.map((e, i) => this.renderToast(e, i, treeStore))
                }
            </Toaster>
        );
    }
}
