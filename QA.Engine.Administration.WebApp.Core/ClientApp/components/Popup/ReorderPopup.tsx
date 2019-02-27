import * as React from 'react';
import TreeStore from 'stores/TreeStore';
import { Card, ButtonGroup, Button, Intent, FormGroup, RadioGroup, Radio } from '@blueprintjs/core';
import PopupStore from 'stores/PopupStore';
import PopupType from 'enums/PopupType';
import TextStore from 'stores/TextStore';
import Texts from 'constants/Texts';
import SiteTreeStore from 'stores/TreeStore/SiteTreeStore';
import { number } from 'prop-types';
import { inject, observer } from 'mobx-react';
import PageSelect from 'components/Select/PageSelect';

interface Props {
    treeStore?: TreeStore;
    popupStore?: PopupStore;
    textStore?: TextStore;
}

interface State {
    isInsertBefore: number;
    relatedItemId: number;
}

@inject('treeStore', 'popupStore', 'textStore')
@observer
export default class ReorderPopup extends React.Component<Props, State> {

    state = { isInsertBefore: 0, relatedItemId: null as number };

    private reorderClick = () => {
        const { treeStore, popupStore } = this.props;
        const { isInsertBefore, relatedItemId } = this.state;
        const siteTreeStore = treeStore.resolveTreeStore() as SiteTreeStore;
        const model: ReorderModel = {
            relatedItemId,
            itemId: popupStore.itemId,
            isInsertBefore: !!isInsertBefore,
        };
        siteTreeStore.reorder(model);
        popupStore.close();
    }

    private changeInsert = (e: React.ChangeEvent<HTMLInputElement>) =>
        this.setState({ isInsertBefore: +e.target.value })

    private changeRelatedItem = (e: PageModel) =>
        this.setState({ relatedItemId: e.id })

    private cancelClick = () =>
        this.props.popupStore.close()

    render() {
        const { popupStore, textStore, treeStore } = this.props;
        const { isInsertBefore } = this.state;

        if (popupStore.type !== PopupType.REORDER) {
            return null;
        }

        const siteTreeStore = treeStore.resolveTreeStore() as SiteTreeStore;
        const pages = siteTreeStore.parentNode.children.filter(x => x.id !== siteTreeStore.selectedNode.id);

        return (
            <Card>
                <FormGroup>
                    <RadioGroup label={textStore.texts[Texts.popupReorderInsertLabel]} selectedValue={isInsertBefore} onChange={this.changeInsert}>
                        <Radio label={textStore.texts[Texts.popupReorderInsertBefore]} value={1}/>
                        <Radio label={textStore.texts[Texts.popupReorderInsertAfter]} value={0}/>
                    </RadioGroup>
                </FormGroup>
                <FormGroup>
                    <PageSelect items={pages} onChange={this.changeRelatedItem} />
                </FormGroup>
                <ButtonGroup className="dialog-button-group">
                    <Button text={textStore.texts[Texts.popupReorderButton]} icon="sort" onClick={this.reorderClick} intent={Intent.SUCCESS} />
                    <Button text={textStore.texts[Texts.popupCancelButton]} icon="undo" onClick={this.cancelClick} />
                </ButtonGroup>
            </Card>
        );
    }
}
