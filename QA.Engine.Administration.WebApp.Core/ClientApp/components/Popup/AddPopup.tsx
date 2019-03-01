import * as React from 'react';
import { Card, Button, FormGroup, InputGroup, ButtonGroup, Intent } from '@blueprintjs/core';
import { observer, inject } from 'mobx-react';
import QpIntegrationStore from 'stores/QpIntegrationStore';
import PopupStore from 'stores/PopupStore';
import DiscriminatorSelect from 'components/Select/DiscriminatorSelect';
import PopupType from 'enums/PopupType';
import TreeStore from 'stores/TreeStore';
import TextStore from 'stores/TextStore';
import Texts from 'constants/Texts';
import TreeStoreType from 'enums/TreeStoreType';
import WidgetTreeStore from 'stores/TreeStore/WidgetTreeStore';

interface Props {
    qpIntegrationStore?: QpIntegrationStore;
    treeStore?: TreeStore;
    popupStore?: PopupStore;
    textStore?: TextStore;
}

interface State {
    discriminator: DiscriminatorModel;
    name: string;
    title: string;
}

@inject('qpIntegrationStore', 'treeStore', 'popupStore', 'textStore')
@observer
export default class AddPopup extends React.Component<Props, State> {

    state = { discriminator: null as DiscriminatorModel, name: '', title: '' };

    private addClick = () => {
        const { popupStore, qpIntegrationStore, treeStore } = this.props;
        const { discriminator, name, title } = this.state;
        let node: PageModel | WidgetModel;
        if (popupStore.type === PopupType.ADD) {
            node = treeStore.getTreeStore(TreeStoreType.SITE).selectedNode;
        }
        if (popupStore.type === PopupType.ADDWIDGET) {
            const widgetTreeStore = treeStore.getTreeStore(TreeStoreType.WIDGET) as WidgetTreeStore;
            if (widgetTreeStore.selectedNode && widgetTreeStore.selectedNode.id === popupStore.itemId) {
                node = widgetTreeStore.selectedNode;
            } else {
                node = widgetTreeStore.selectedSiteTreeNode;
            }
        }
        qpIntegrationStore.add(node, null, name, title, discriminator.id, 0);
        popupStore.close();
    }

    private cancelClick = () => {
        const { popupStore } = this.props;
        popupStore.close();
    }

    private changeDiscriminator = (e: DiscriminatorModel) =>
        this.setState({ discriminator: e })

    private changeTitle = (e: React.ChangeEvent<HTMLInputElement>) =>
        this.setState({ title: e.target.value })

    private changeName = (e: React.ChangeEvent<HTMLInputElement>) =>
        this.setState({ name: e.target.value })

    render() {
        const { popupStore, textStore } = this.props;
        const { name, title } = this.state;
        if (popupStore.type !== PopupType.ADD && popupStore.type !== PopupType.ADDWIDGET) {
            return null;
        }

        return (
            <Card>
                <FormGroup label={textStore.texts[Texts.popupFieldTitle]}>
                    <InputGroup placeholder={textStore.texts[Texts.popupFieldTitlePlaceholder]} value={title} onChange={this.changeTitle} />
                </FormGroup>
                <FormGroup label={textStore.texts[Texts.popupFieldAlias]}>
                    <InputGroup placeholder={textStore.texts[Texts.popupFieldAliasPlaceholder]} value={name} onChange={this.changeName} />
                </FormGroup>
                <FormGroup label={textStore.texts[Texts.popupFieldContentType]}>
                    <DiscriminatorSelect items={popupStore.discriminators} onChange={this.changeDiscriminator} />
                </FormGroup>
                <ButtonGroup className="dialog-button-group">
                    <Button text={textStore.texts[Texts.popupAddButton]} icon="new-object" onClick={this.addClick} intent={Intent.SUCCESS} />
                    <Button text={textStore.texts[Texts.popupCancelButton]} icon="undo" onClick={this.cancelClick} />
                </ButtonGroup>
            </Card>
        );
    }
}
