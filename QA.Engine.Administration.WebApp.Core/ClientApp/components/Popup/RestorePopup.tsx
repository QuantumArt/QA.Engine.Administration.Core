import * as React from 'react';
import { Card, Button, Checkbox, ButtonGroup, Intent, FormGroup } from '@blueprintjs/core';
import { observer, inject } from 'mobx-react';
import PopupStore from 'stores/PopupStore';
import PopupType from 'enums/PopupType';
import TreeStore from 'stores/TreeStore';
import ArchiveTreeStore from 'stores/TreeStore/ArchiveTreeStore';

interface Props {
    treeStore?: TreeStore;
    popupStore?: PopupStore;
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
        (treeStore.resolveTreeStore() as ArchiveTreeStore).restore(model);
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
                <FormGroup>
                    <Checkbox checked={restoreAllVersions} onChange={this.changeRestoreAllVersions}>
                        Восстановить версии
                    </Checkbox>
                    <Checkbox checked={restoreChildren} onChange={this.changeRestoreChildren}>
                        Восстановить вложенные страницы
                    </Checkbox>
                    <Checkbox checked={restoreWidgets} onChange={this.changeRestoreWidgets}>
                        Восстановить виджеты
                    </Checkbox>
                    <Checkbox checked={restoreContentVersions} onChange={this.changeRestoreContentVersions}>
                        Восстановить контентные версии
                    </Checkbox>
                </FormGroup>
                <ButtonGroup className="dialog-button-group">
                    <Button text="Восстановить" icon="swap-horizontal" onClick={this.restoreClick} intent={Intent.SUCCESS} />
                    <Button text="cancel" icon="undo" onClick={this.cancelClick} />
                </ButtonGroup>
            </Card>
        );
    }
}
