import * as React from 'react';
import { Card, Spinner, Button, FormGroup, InputGroup, RadioGroup, Radio } from '@blueprintjs/core';
import { observer, inject } from 'mobx-react';
import { QpIntegrationState, VersionType } from 'stores/QpIntegrationStore';
import { PopupState } from 'stores/PopupStore';
import OperationState from 'enums/OperationState';
import DiscriminatorSelect from 'components/Select/DiscriminatorSelect';
import { SiteTreeState } from 'stores/SiteTreeStore';
import PopupType from 'enums/PopupType';

interface Props {
    qpIntegrationStore?: QpIntegrationState;
    siteTreeStore?: SiteTreeState;
    popupStore?: PopupState;
}

interface State {
    discriminator: DiscriminatorModel;
    version: VersionType;
}

@inject('qpIntegrationStore', 'siteTreeStore', 'popupStore')
@observer
export default class AddVersionPopup extends React.Component<Props, State> {

    state = {
        discriminator: null as DiscriminatorModel,
        version: null as VersionType,
    };

    private addClick = () => {
        const { popupStore, qpIntegrationStore, siteTreeStore } = this.props;
        const { discriminator, version } = this.state;
        const node = siteTreeStore.getNodeById(popupStore.itemId);
        qpIntegrationStore.add(node, version, node.alias, node.title, discriminator.id, 0);
        popupStore.close();
    }

    private cancelClick = () => {
        const { popupStore } = this.props;
        popupStore.close();
    }

    private changeDiscriminator = (e: DiscriminatorModel) =>
        this.setState({ discriminator: e })

    private changeVersion = (version: React.ChangeEvent<HTMLInputElement>) =>
        this.setState({ version: version.target.value as VersionType })

    render() {
        const { popupStore } = this.props;
        const { version } = this.state;

        if (popupStore.type !== PopupType.ADDVERSION) {
            return null;
        }

        if (popupStore.state === OperationState.NONE || popupStore.state === OperationState.PENDING) {
            return (<Spinner size={30} />);
        }

        const discriminators = popupStore.discriminators.filter(x => x.isPage === true);

        return (
            <Card>
                <RadioGroup label="Title" selectedValue={version} onChange={this.changeVersion}>
                    <Radio label="Content" value={VersionType.Content}></Radio>
                    <Radio label="Structural" value={VersionType.Structural}></Radio>
                </RadioGroup>
                <div>
                    <DiscriminatorSelect items={discriminators} onChange={this.changeDiscriminator} />
                </div>
                <div>
                    <Button text="add" onClick={this.addClick} />
                    <Button text="cancel" onClick={this.cancelClick} />
                </div>
            </Card>
        );
    }
}
