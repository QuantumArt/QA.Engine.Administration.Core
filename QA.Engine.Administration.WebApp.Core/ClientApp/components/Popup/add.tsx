import * as React from 'react';
import { Card, Spinner, Button, FormGroup, InputGroup } from '@blueprintjs/core';
import { observer, inject } from 'mobx-react';
import { QpIntegrationState } from 'stores/QpIntegrationStore';
import { PopupState } from 'stores/PopupStore';
import OperationState from 'enums/OperationState';
import DiscriminatorSelect from 'components/Select/DiscriminatorSelect';
import { SiteTreeState } from 'stores/SiteTreeStore';

interface Props {
    qpIntegrationStore?: QpIntegrationState;
    siteTreeStore?: SiteTreeState;
    popupStore?: PopupState;
}

interface State {
    discriminator: DiscriminatorModel;
    name: string;
    title: string;
}

@inject('qpIntegrationStore', 'siteTreeStore', 'popupStore')
@observer
export default class AddPopup extends React.Component<Props, State> {

    constructor(props: any) {
        super(props);
        this.state = { discriminator: null, name: '', title: '' };
    }

    private addClick = () => {
        const { popupStore, qpIntegrationStore, siteTreeStore } = this.props;
        const { discriminator, name, title } = this.state;
        const node = siteTreeStore.getNodeById(popupStore.itemId);
        qpIntegrationStore.add(node, null, name, title, discriminator.id, 0);
    }

    private cancelClick = () => {
        const { popupStore } = this.props;
        popupStore.close();
    }

    private changeDiscriminator = (e: DiscriminatorModel) => {
        this.setState({ discriminator: e });
    }

    private changeTitle = (e: React.ChangeEvent<HTMLInputElement>) => {
        this.setState({ title: e.target.value });
    }

    private changeName = (e: React.ChangeEvent<HTMLInputElement>) => {
        this.setState({ name: e.target.value });
    }

    render() {
        const { popupStore } = this.props;
        const { name, title } = this.state;

        if (popupStore.state === OperationState.NONE || popupStore.state === OperationState.PENDING) {
            return (<Spinner size={30} />);
        }

        return (
            <Card>
                <FormGroup label="Title">
                    <InputGroup placeholder="Название раздела" value={title} onChange={this.changeTitle}></InputGroup>
                </FormGroup>
                <FormGroup label="Name">
                    <InputGroup placeholder="alias" value={name} onChange={this.changeName}></InputGroup>
                </FormGroup>
                <div>
                    <DiscriminatorSelect items={popupStore.discriminators} onChange={this.changeDiscriminator} />
                </div>
                <div>
                    <Button text="add" onClick={this.addClick} />
                    <Button text="cancel" onClick={this.cancelClick} />
                </div>
            </Card>
        );
    }
}
