import * as React from 'react';
import TreeStore from 'stores/TreeStore';
import { Card, ButtonGroup, Button, Intent, FormGroup } from '@blueprintjs/core';
import PopupStore from 'stores/PopupStore';
import PopupType from 'enums/PopupType';
import TextStore from 'stores/TextStore';
import Texts from 'constants/Texts';
import { inject, observer } from 'mobx-react';
import PageSelect from 'components/Select/PageSelect';

interface Props {
    treeStore?: TreeStore;
    popupStore?: PopupStore;
    textStore?: TextStore;
}

interface State {
    newParent: PageModel;
    newParentIntent: Intent;
}

@inject('treeStore', 'popupStore', 'textStore')
@observer
export default class MovePopup extends React.Component<Props, State> {

    state = { newParent: null as PageModel, ...this.resetIntent };

    private moveClick = () => {
        const { treeStore, popupStore } = this.props;
        const { newParent } = this.state;
        if (newParent == null) {
            this.setState({ newParentIntent: Intent.DANGER });
            return;
        }
        const model: MoveModel = {
            newParentId: newParent.id,
            itemId: popupStore.itemId,
        };
        treeStore.move(model);
        popupStore.close();
    }

    private changeNewParent = (e: PageModel) =>
        this.setState({ newParent: e, ...this.resetIntent })

    private cancelClick = () =>
        this.props.popupStore.close()

    render() {
        const { popupStore, textStore, treeStore } = this.props;
        const { newParentIntent } = this.state;

        if (popupStore.type !== PopupType.MOVE) {
            return null;
        }

        const siteTreeStore = treeStore.getSiteTreeStore();
        const pages = siteTreeStore.flatPages;

        return (
            <Card>
                <FormGroup>
                    <PageSelect items={pages} filterable onChange={this.changeNewParent} intent={newParentIntent} />
                </FormGroup>
                <ButtonGroup className="dialog-button-group">
                    <Button text={textStore.texts[Texts.popupMoveButton]} icon="move" onClick={this.moveClick} intent={Intent.SUCCESS} />
                    <Button text={textStore.texts[Texts.popupCancelButton]} icon="undo" onClick={this.cancelClick} />
                </ButtonGroup>
            </Card>
        );
    }

    private resetIntent = { newParentIntent: Intent.NONE };
}
