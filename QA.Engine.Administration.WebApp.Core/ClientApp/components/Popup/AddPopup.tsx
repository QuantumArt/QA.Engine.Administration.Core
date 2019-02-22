import * as React from 'react';
import { Card, Button, FormGroup, InputGroup, ButtonGroup, Intent } from '@blueprintjs/core';
import { observer, inject } from 'mobx-react';
import QpIntegrationStore from 'stores/QpIntegrationStore';
import PopupStore from 'stores/PopupStore';
import DiscriminatorSelect from 'components/Select/DiscriminatorSelect';
import PopupType from 'enums/PopupType';
import TreeStore from 'stores/TreeStore';

interface Props {
    qpIntegrationStore?: QpIntegrationStore;
    treeStore?: TreeStore;
    popupStore?: PopupStore;
}

interface State {
    discriminator: DiscriminatorModel;
    name: string;
    title: string;
}

@inject('qpIntegrationStore', 'treeStore', 'popupStore')
@observer
export default class AddPopup extends React.Component<Props, State> {

    state = { discriminator: null as DiscriminatorModel, name: '', title: '' };

    private addClick = () => {
        const { popupStore, qpIntegrationStore, treeStore } = this.props;
        const { discriminator, name, title } = this.state;
        const node = treeStore.resolveTreeStore().selectedNode as PageModel;
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
        const { popupStore } = this.props;
        const { name, title } = this.state;
        if (popupStore.type !== PopupType.ADD) {
            return null;
        }
        const discriminators = popupStore.discriminators.filter(x => x.isPage === true);

        return (
            <Card>
                <FormGroup label="Title">
                    <InputGroup placeholder="Название раздела" value={title} onChange={this.changeTitle} />
                </FormGroup>
                <FormGroup label="Name">
                    <InputGroup placeholder="alias" value={name} onChange={this.changeName} />
                </FormGroup>
                <FormGroup label="Type">
                    <DiscriminatorSelect items={discriminators} onChange={this.changeDiscriminator} />
                </FormGroup>
                <ButtonGroup className="dialog-button-group">
                    <Button text="Добавить" icon="new-object" onClick={this.addClick} intent={Intent.SUCCESS} />
                    <Button text="Отмена" icon="undo" onClick={this.cancelClick} />
                </ButtonGroup>
            </Card>
        );
    }
}