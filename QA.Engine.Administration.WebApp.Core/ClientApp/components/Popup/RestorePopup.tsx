import * as React from 'react';
import { Card, Button, Checkbox, ButtonGroup, Intent, FormGroup } from '@blueprintjs/core';
import { observer, inject } from 'mobx-react';
import PopupStore from 'stores/PopupStore';
import PopupType from 'enums/PopupType';
import TreeStore from 'stores/TreeStore';
import TextStore from 'stores/TextStore';
import Texts from 'constants/Texts';

interface Props {
    treeStore?: TreeStore;
    popupStore?: PopupStore;
    textStore?: TextStore;
}

interface State {
    restoreAllVersions: boolean;
    restoreChildren: boolean;
    restoreContentVersions: boolean;
    restoreWidgets: boolean;
}

@inject('treeStore', 'popupStore', 'textStore')
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
        treeStore.restore(model);
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
        const { popupStore, textStore, treeStore } = this.props;
        const { restoreAllVersions, restoreChildren, restoreContentVersions, restoreWidgets } = this.state;
        const isPage = treeStore.getArchiveTreeStore().IsPage(popupStore.itemId);
        if (popupStore.type !== PopupType.RESTORE) {
            return null;
        }

        return (
            <Card>
                <FormGroup>
                    <Checkbox checked={restoreAllVersions} onChange={this.changeRestoreAllVersions}>
                        {textStore.texts[Texts.popupRestoreAllVersion]}
                    </Checkbox>
                    {isPage &&
                        <Checkbox checked={restoreChildren} onChange={this.changeRestoreChildren}>
                            {textStore.texts[Texts.popupRestoreChildren]}
                        </Checkbox>
                    }
                    <Checkbox checked={restoreWidgets} onChange={this.changeRestoreWidgets}>
                        {textStore.texts[Texts.popupRestoreWidget]}
                    </Checkbox>
                    <Checkbox checked={restoreContentVersions} onChange={this.changeRestoreContentVersions}>
                        {textStore.texts[Texts.popupRestoreContentVersion]}
                    </Checkbox>
                </FormGroup>
                <ButtonGroup className="dialog-button-group">
                    <Button text={textStore.texts[Texts.popupRestoreButton]} icon="swap-horizontal" onClick={this.restoreClick} intent={Intent.SUCCESS} />
                    <Button text={textStore.texts[Texts.popupCancelButton]} icon="undo" onClick={this.cancelClick} />
                </ButtonGroup>
            </Card>
        );
    }
}
