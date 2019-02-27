import * as React from 'react';
import { inject, observer } from 'mobx-react';
import { Intent, Toast, Toaster } from '@blueprintjs/core';
import OperationState from 'enums/OperationState';
import TreeStore from 'stores/TreeStore';
import EditArticleStore from 'stores/EditArticleStore';
import TreeErrors from 'enums/TreeErrors';
import { TreeErrorModel } from 'stores/TreeStore/BaseTreeStore';
import SiteTreeStore from 'stores/TreeStore/SiteTreeStore';
import ArchiveTreeStore from 'stores/TreeStore/ArchiveTreeStore';
import WidgetTreeStore from 'stores/TreeStore/WidgetTreeStore';
import ContentVersionTreeStore from 'stores/TreeStore/ContentVersionTreeStore';

interface Props {
    treeStore?: TreeStore;
    editArticleStore?: EditArticleStore;
}

type CurrentTree = SiteTreeStore | ArchiveTreeStore | WidgetTreeStore | ContentVersionTreeStore;

@inject('treeStore', 'editArticleStore')
@observer
export default class ErrorToast extends React.Component<Props> {

    private handleTreeErrorClick = (tree: CurrentTree) => () => {
        tree.treeErrors.forEach((e) => {
            switch (e.type) {
                case TreeErrors.fetch:
                    if (tree instanceof SiteTreeStore ||
                        tree instanceof ArchiveTreeStore ||
                        tree instanceof WidgetTreeStore ||
                        tree instanceof ContentVersionTreeStore
                    ) {
                        tree.fetchTree();
                    }
                    break;
                case TreeErrors.update:
                    if (tree instanceof SiteTreeStore ||
                        tree instanceof ArchiveTreeStore ||
                        tree instanceof WidgetTreeStore ||
                        tree instanceof ContentVersionTreeStore
                    ) {
                        tree.updateSubTree(e.data);
                    }
                    break;
                case TreeErrors.publish:
                    if (tree instanceof SiteTreeStore ||
                        tree instanceof WidgetTreeStore ||
                        tree instanceof ContentVersionTreeStore
                    ) {
                        tree.publish(e.data);
                    }
                    break;
                case TreeErrors.archive:
                    if (tree instanceof SiteTreeStore) {
                        tree.archive(e.data);
                    }
                    break;
                case TreeErrors.edit:
                    if (tree instanceof SiteTreeStore) {
                        tree.edit(e.data);
                    }
                    break;
                case TreeErrors.restore:
                    if (tree instanceof ArchiveTreeStore) {
                        tree.restore(e.data);
                    }
                    break;
                case TreeErrors.delete:
                    if (tree instanceof ArchiveTreeStore) {
                        tree.delete(e.data);
                    }
                    break;
                default:
                    break;
            }
        });
    }

    private handleDismiss = (i: number, cb: (i: number) => void) => () => {
        cb(i);
    }

    private renderToast = (e:TreeErrorModel, i: number, currentTree: CurrentTree) => (
        <Toast
            message={`${e.type}. ${e.message}`}
            icon="warning-sign"
            intent={Intent.DANGER}
            action={{
                onClick: this.handleTreeErrorClick(currentTree),
                icon: 'repeat',
            }}
            onDismiss={this.handleDismiss(i, currentTree.removeError)}
            key={e.id}
        />
    )

    render() {
        const { treeStore } = this.props;
        const siteTree = treeStore.resolveTreeStore();
        const widgetTree = treeStore.getWidgetStore();
        const contentVersionsStore = treeStore.getContentVersionsStore();

        return (
            <Toaster>
                {siteTree.treeState === OperationState.ERROR &&
                    siteTree.treeErrors.map((e, i) => this.renderToast(e, i, siteTree))
                }
                {widgetTree.treeState === OperationState.ERROR &&
                    widgetTree.treeErrors.map((e, i) => this.renderToast(e, i, widgetTree))
                }
                {contentVersionsStore.treeState === OperationState.ERROR &&
                    contentVersionsStore.treeErrors.map((e, i) => this.renderToast(e, i, contentVersionsStore))
                }
            </Toaster>
        );
    }
}
