import * as React from 'react';
import { Card, Button, Checkbox } from '@blueprintjs/core';
import { observer, inject } from 'mobx-react';
import { PopupState } from 'stores/PopupStore';
import PopupType from 'enums/PopupType';
import TreeStore from 'stores/TreeStore';
import { ArchiveState } from 'stores/ArchiveStore';

interface Props {
    treeStore?: TreeStore;
    popupStore?: PopupState;
}

interface State {
    deleteAllVersions: boolean;
}

enum ContentVersionOperations {
    archive = 'archive',
    move = 'move',
}

@inject('treeStore', 'popupStore')
@observer
export default class DeletePopup extends React.Component<Props, State> {

    state = {
        deleteAllVersions: false,
        deleteContentVersions: ContentVersionOperations.archive,
        contentVersionId: null as number,
    };

    private deleteClick = () => {
        const { popupStore, treeStore } = this.props;
        const { deleteAllVersions } = this.state;
        const model: DeleteModel = {
            itemId: popupStore.itemId,
            isDeleteAllVersions: deleteAllVersions,
        };
        (treeStore.resolveTreeStore() as ArchiveState).delete(model);
        popupStore.close();
    }

    private cancelClick = () => this.props.popupStore.close();

    private changeDeleteAllVersions = (version: React.ChangeEvent<HTMLInputElement>) =>
        this.setState({ deleteAllVersions: version.target.checked })

    render() {
        const { popupStore } = this.props;
        const { deleteAllVersions } = this.state;

        if (popupStore.type !== PopupType.DELETE) {
            return null;
        }

        return (
            <Card>
                <Checkbox
                    checked={deleteAllVersions}
                    onChange={this.changeDeleteAllVersions}
                >
                    Remove versions
                </Checkbox>
                <div>
                    <Button text="delete" onClick={this.deleteClick} />
                    <Button text="cancel" onClick={this.cancelClick} />
                </div>
            </Card>
        );
    }
}
