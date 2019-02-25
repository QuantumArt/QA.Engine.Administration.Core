import * as React from 'react';
import { inject, observer } from 'mobx-react';
import { Intent, Toast, Toaster } from '@blueprintjs/core';
import OperationState from 'enums/OperationState';
import TreeStore from 'stores/TreeStore';
import EditArticleStore from 'stores/EditArticleStore';
import TreeErrors from 'enums/TreeErrors';
import SiteTreeStore from 'stores/TreeStore/SiteTreeStore';
import ArchiveTreeStore from 'stores/TreeStore/ArchiveTreeStore';

interface Props {
    treeStore?: TreeStore;
    editArticleStore?: EditArticleStore;
}

@inject('treeStore', 'editArticleStore')
@observer
export default class ErrorToast extends React.Component<Props> {

    private handleTreeErrorClick = (i: number) => () => {
        const { treeStore } = this.props;
        const tree = treeStore.resolveTreeStore();
        tree.treeErrors.forEach((e) => {
            switch (e.type) {
                case TreeErrors.fetch:
                    if (tree instanceof SiteTreeStore) {
                        tree.fetchTree();
                    }
                    break;
                case TreeErrors.update:
                    if (tree instanceof SiteTreeStore) {
                        tree.updateSubTree(e.data);
                    }
                    break;
                case TreeErrors.publish:
                    if (tree instanceof SiteTreeStore) {
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

    render() {
        const { treeStore } = this.props;
        const tree = treeStore.resolveTreeStore();

        return (
            <Toaster>
                {tree.treeState === OperationState.ERROR && tree.treeErrors.map((e, i) => (
                    <Toast
                        message={e.type}
                        icon="warning-sign"
                        intent={Intent.DANGER}
                        action={{
                            onClick: this.handleTreeErrorClick(i),
                            icon: 'repeat',
                        }}
                        onDismiss={this.handleDismiss(i, tree.removeError)}
                    />
                ))}
            </Toaster>
        );
    }
}
