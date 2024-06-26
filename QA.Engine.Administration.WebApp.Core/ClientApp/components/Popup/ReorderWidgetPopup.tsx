import * as React from 'react';
import TreeStore from 'stores/TreeStore';
import { Card, ButtonGroup, Button, Intent, FormGroup, RadioGroup, Radio } from '@blueprintjs/core';
import PopupStore from 'stores/PopupStore';
import PopupType from 'enums/PopupType';
import TextStore from 'stores/TextStore';
import Texts from 'constants/Texts';
import { inject, observer } from 'mobx-react';
import WidgetSelect from "components/Select/WidgetSelect";

interface Props {
    treeStore?: TreeStore;
    popupStore?: PopupStore;
    textStore?: TextStore;
}

interface State {
    isInsertBefore: number;
    relatedItem: WidgetModel;
    relatedItemIntent: Intent;
}

@inject('treeStore', 'popupStore', 'textStore')
@observer
export default class ReorderWidgetPopup extends React.Component<Props, State> {

    private resetIntent = { relatedItemIntent: Intent.NONE };
    state = { isInsertBefore: 0, relatedItem: null as WidgetModel, ...this.resetIntent };

    private reorderClick = () => {
        const { treeStore, popupStore } = this.props;
        const { isInsertBefore, relatedItem } = this.state;
        if (relatedItem == null) {
            this.setState({ relatedItemIntent: Intent.DANGER });
            return;
        }
        const model: ReorderModel = {
            relatedItemId: relatedItem.id,
            itemId: popupStore.itemId,
            isInsertBefore: !!isInsertBefore,
        };
        treeStore.reorder(model);
        popupStore.close();
    }

    private changeInsert = (e: React.ChangeEvent<HTMLInputElement>) =>
        this.setState({ isInsertBefore: +e.target.value, ...this.resetIntent })

    private changeRelatedItem = (e: WidgetModel) =>
        this.setState({ relatedItem: e, ...this.resetIntent })

    private cancelClick = () =>
        this.props.popupStore.close()

    render() {
        const { popupStore, textStore, treeStore } = this.props;
        const { isInsertBefore, relatedItemIntent } = this.state;

        if (popupStore.type !== PopupType.REORDERWIDGET) {
            return null;
        }

        const widgetTreeStore = treeStore.getWidgetTreeStore();
        const widgets = widgetTreeStore.widgetsInZone.filter((x) => x.id !== widgetTreeStore.selectedNode.id);

        return (
            <Card>
                <FormGroup>
                    <RadioGroup label={textStore.texts[Texts.popupReorderInsertLabel]} selectedValue={isInsertBefore} onChange={this.changeInsert}>
                        <Radio label={textStore.texts[Texts.popupReorderInsertBefore]} value={1}/>
                        <Radio label={textStore.texts[Texts.popupReorderInsertAfter]} value={0}/>
                    </RadioGroup>
                </FormGroup>
                <FormGroup>
                    <WidgetSelect items={widgets} onChange={this.changeRelatedItem} intent={relatedItemIntent} />
                </FormGroup>
                <ButtonGroup className="dialog-button-group">
                    <Button text={textStore.texts[Texts.popupReorderButton]} icon="sort" onClick={this.reorderClick} intent={Intent.SUCCESS} />
                    <Button text={textStore.texts[Texts.popupCancelButton]} icon="undo" onClick={this.cancelClick} />
                </ButtonGroup>
            </Card>
        );
    }
}
