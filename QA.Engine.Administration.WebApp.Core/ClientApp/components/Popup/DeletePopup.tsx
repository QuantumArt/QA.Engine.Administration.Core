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
    deleteAllVersions: boolean;
}

@inject('treeStore', 'popupStore', 'textStore')
@observer
export default class DeletePopup extends React.Component<Props, State> {

    state = {
        deleteAllVersions: false,
        contentVersionId: null as number,
    };

    private deleteClick = () => {
        const { popupStore, treeStore } = this.props;
        const { deleteAllVersions } = this.state;
        const model: DeleteModel = {
            itemId: popupStore.itemId,
            isDeleteAllVersions: deleteAllVersions,
        };
        treeStore.delete(model);
        popupStore.close();
    }

    private cancelClick = () => this.props.popupStore.close();

    private changeDeleteAllVersions = (version: React.ChangeEvent<HTMLInputElement>) =>
        this.setState({ deleteAllVersions: version.target.checked })

    render() {
        const { popupStore, textStore } = this.props;
        const { deleteAllVersions } = this.state;
        if (popupStore.type !== PopupType.DELETE) {
            return null;
        }

        return (
            <Card>
                <FormGroup>
                    <Checkbox checked={deleteAllVersions} onChange={this.changeDeleteAllVersions}>
                        {textStore.texts[Texts.popupDeleteAllVersion]}
                    </Checkbox>
                </FormGroup>
                <ButtonGroup className="dialog-button-group">
                    <Button text={textStore.texts[Texts.popupDeleteButton]} icon="delete" onClick={this.deleteClick} intent={Intent.DANGER} />
                    <Button text={textStore.texts[Texts.popupCancelButton]} icon="undo" onClick={this.cancelClick} />
                </ButtonGroup>
            </Card>
        );
    }
}
