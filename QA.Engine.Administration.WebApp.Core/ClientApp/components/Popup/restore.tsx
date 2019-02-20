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
    restoreAllVersions: boolean;
    restoreChildren: boolean;
    restoreContentVersions: boolean;
    restoreWidgets: boolean;
}

@inject('treeStore', 'popupStore')
@observer
export default class RestorePopup extends React.Component<Props, State> {

    state = {
        restoreAllVersions: false,
        restoreChildren: false,
        restoreContentVersions: false,
        restoreWidgets: false,
    };

    private restoreClick = () => {
        const { popupStore, treeStore } = this.props;
        const { restoreAllVersions, restoreChildren, restoreContentVersions, restoreWidgets } = this.state;
        const model: RestoreModel = {
            itemId: popupStore.itemId,
            isRestoreAllVersions: restoreAllVersions,
            isRestoreChildren: restoreChildren,
            isRestoreContentVersions: restoreContentVersions,
            isRestoreWidgets: restoreWidgets,
        };
        (treeStore.resolveTreeStore() as ArchiveState).restore(model);
        popupStore.close();
    }

    private cancelClick = () => this.props.popupStore.close();

    private changeRestoreAllVersions = (version: React.ChangeEvent<HTMLInputElement>) =>
        this.setState({ restoreAllVersions: version.target.checked })

    private changeRestoreChildren = (version: React.ChangeEvent<HTMLInputElement>) =>
        this.setState({ restoreChildren: version.target.checked })

    private changeRestoreWidgets = (version: React.ChangeEvent<HTMLInputElement>) =>
        this.setState({ restoreWidgets: version.target.checked })

    private changeRestoreContentVersions = (version: React.ChangeEvent<HTMLInputElement>) =>
        this.setState({ restoreContentVersions: version.target.checked })

    render() {
        const { popupStore } = this.props;
        const { restoreAllVersions, restoreChildren, restoreContentVersions, restoreWidgets } = this.state;

        if (popupStore.type !== PopupType.RESTORE) {
            return null;
        }

        return (
            <Card>
                <Checkbox checked={restoreAllVersions} onChange={this.changeRestoreAllVersions}>
                    Restore versions
                </Checkbox>
                <Checkbox checked={restoreChildren} onChange={this.changeRestoreChildren}>
                    Restore children
                </Checkbox>
                <Checkbox checked={restoreWidgets} onChange={this.changeRestoreWidgets}>
                    Restore widgets
                </Checkbox>
                <Checkbox checked={restoreContentVersions} onChange={this.changeRestoreContentVersions}>
                    Restore content versions
                </Checkbox>
                <div>
                    <Button text="restore" onClick={this.restoreClick} />
                    <Button text="cancel" onClick={this.cancelClick} />
                </div>
            </Card>
        );
    }
}
