import * as React from 'react';
import { Button, ButtonGroup, Card, FormGroup, Intent, Radio, RadioGroup } from '@blueprintjs/core';
import { inject, observer } from 'mobx-react';
import { QpIntegrationState, VersionType } from 'stores/QpIntegrationStore';
import { PopupState } from 'stores/PopupStore';
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
        const discriminators = popupStore.discriminators.filter(x => x.isPage === true);

        return (
            <Card>
                <FormGroup>
                    <RadioGroup label="Версия" selectedValue={version} onChange={this.changeVersion}>
                        <Radio label="Контентная" value={VersionType.Content}/>
                        <Radio label="Структурная" value={VersionType.Structural}/>
                    </RadioGroup>
                </FormGroup>
                <FormGroup>
                    <DiscriminatorSelect items={discriminators} onChange={this.changeDiscriminator}/>
                </FormGroup>
                <ButtonGroup className="dialog-button-group">
                    <Button text="Добавить" icon="add" onClick={this.addClick} intent={Intent.SUCCESS}/>
                    <Button text="Отмена" icon="undo" onClick={this.cancelClick}/>
                </ButtonGroup>
            </Card>
        );
    }
}
