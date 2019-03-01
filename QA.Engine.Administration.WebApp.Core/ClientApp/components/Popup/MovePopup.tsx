import * as React from 'react';
import TreeStore from 'stores/TreeStore';
import { Card, ButtonGroup, Button, Intent, FormGroup } from '@blueprintjs/core';
import PopupStore from 'stores/PopupStore';
import PopupType from 'enums/PopupType';
import TextStore from 'stores/TextStore';
import Texts from 'constants/Texts';
import SiteTreeStore from 'stores/TreeStore/SiteTreeStore';
import { inject, observer } from 'mobx-react';
import PageSelect from 'components/Select/PageSelect';
import TreeStoreType from 'enums/TreeStoreType';

interface Props {
    treeStore?: TreeStore;
    popupStore?: PopupStore;
    textStore?: TextStore;
}

interface State {
    newParentId: number;
}

@inject('treeStore', 'popupStore', 'textStore')
@observer
export default class MovePopup extends React.Component<Props, State> {

    state = { newParentId: null as number };

    private moveClick = () => {
        const { treeStore, popupStore } = this.props;
        const { newParentId } = this.state;
        const model: MoveModel = {
            newParentId,
            itemId: popupStore.itemId,
        };
        treeStore.move(model);
        popupStore.close();
    }

    private changeNewParent = (e: PageModel) =>
        this.setState({ newParentId: e.id })

    private cancelClick = () =>
        this.props.popupStore.close()

    render() {
        const { popupStore, textStore, treeStore } = this.props;

        if (popupStore.type !== PopupType.MOVE) {
            return null;
        }

        const siteTreeStore = treeStore.getSiteTreeStore();
        const pages = siteTreeStore.flatPages;

        return (
            <Card>
                <FormGroup>
                    <PageSelect items={pages} filterable onChange={this.changeNewParent} />
                </FormGroup>
                <ButtonGroup className="dialog-button-group">
                    <Button text={textStore.texts[Texts.popupMoveButton]} icon="move" onClick={this.moveClick} intent={Intent.SUCCESS} />
                    <Button text={textStore.texts[Texts.popupCancelButton]} icon="undo" onClick={this.cancelClick} />
                </ButtonGroup>
            </Card>
        );
    }
}
