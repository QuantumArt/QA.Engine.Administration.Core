import * as React from 'react';
import { Button, ButtonGroup, Card, FormGroup, InputGroup, Intent } from '@blueprintjs/core';
import { inject, observer } from 'mobx-react';
import QpIntegrationStore from 'stores/QpIntegrationStore';
import PopupStore from 'stores/PopupStore';
import DiscriminatorSelect from 'components/Select/DiscriminatorSelect';
import PopupType from 'enums/PopupType';
import TreeStore from 'stores/TreeStore';
import TextStore from 'stores/TextStore';
import Texts from 'constants/Texts';

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
    discriminatorIntent: Intent;
    nameIntent: Intent;
    titleIntent: Intent;
}

@inject('qpIntegrationStore', 'treeStore', 'popupStore', 'textStore')
@observer
export default class AddPopup extends React.Component<Props, State> {

    private resetIntent = { discriminatorIntent: Intent.NONE, nameIntent: Intent.NONE, titleIntent: Intent.NONE };
    state = { discriminator: null as DiscriminatorModel, name: '', title: '', ...this.resetIntent };

    private addClick = () => {
        const { popupStore, qpIntegrationStore, treeStore } = this.props;
        const { discriminator, name, title } = this.state;
        const isNullOrWhitespace = (text: string): boolean => text == null || text.trim() === '';
        const isNamedOrWidgetPopup = popupStore.type === PopupType.ADDWIDGET || !isNullOrWhitespace(name);
        if (isNullOrWhitespace(title) || !isNamedOrWidgetPopup || discriminator == null) {
            this.setState({
                titleIntent: isNullOrWhitespace(title) ? Intent.DANGER : Intent.NONE,
                nameIntent: isNamedOrWidgetPopup ? Intent.NONE : Intent.DANGER,
                discriminatorIntent: discriminator == null ? Intent.DANGER : Intent.NONE,
            });
            return;
        }
        let node: PageModel | WidgetModel;
        if (popupStore.type === PopupType.ADD) {
            node = treeStore.getSiteTreeStore().getNode(popupStore.itemId);
        }
        if (popupStore.type === PopupType.ADDWIDGET) {
            const widgetTreeStore = treeStore.getWidgetTreeStore();
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
        this.setState({ discriminator: e, ...this.resetIntent })

    private changeTitle = (e: React.ChangeEvent<HTMLInputElement>) =>
        this.setState({ title: e.target.value, ...this.resetIntent })

    private changeName = (e: React.ChangeEvent<HTMLInputElement>) =>
        this.setState({ name: e.target.value, ...this.resetIntent })

    render() {
        const { popupStore, textStore } = this.props;
        const { name, title } = this.state;
        const { titleIntent, nameIntent, discriminatorIntent } = this.state;
        if (popupStore.type !== PopupType.ADD && popupStore.type !== PopupType.ADDWIDGET) {
            return null;
        }

        return (
            <Card>
                <FormGroup label={textStore.texts[Texts.popupFieldTitle]}>
                    <InputGroup placeholder={textStore.texts[Texts.popupFieldTitlePlaceholder]} value={title} onChange={this.changeTitle} intent={titleIntent} />
                </FormGroup>
                <FormGroup label={textStore.texts[Texts.popupFieldAlias]}>
                    <InputGroup placeholder={textStore.texts[Texts.popupFieldAliasPlaceholder]} value={name} onChange={this.changeName} intent={nameIntent} />
                </FormGroup>
                <FormGroup label={textStore.texts[Texts.popupFieldContentType]}>
                    <DiscriminatorSelect items={popupStore.discriminators} onChange={this.changeDiscriminator} intent={discriminatorIntent} />
                </FormGroup>
                <ButtonGroup className="dialog-button-group">
                    <Button text={textStore.texts[Texts.popupAddButton]} icon="new-object" onClick={this.addClick} intent={Intent.SUCCESS} />
                    <Button text={textStore.texts[Texts.popupCancelButton]} icon="undo" onClick={this.cancelClick} />
                </ButtonGroup>
            </Card>
        );
    }
}
