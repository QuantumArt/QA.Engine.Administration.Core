import * as React from 'react';
import { Button, ButtonGroup, Card, FormGroup, Intent, Radio, RadioGroup } from '@blueprintjs/core';
import { inject, observer } from 'mobx-react';
import QpIntegrationStore, { VersionType } from 'stores/QpIntegrationStore';
import PopupStore from 'stores/PopupStore';
import DiscriminatorSelect from 'components/Select/DiscriminatorSelect';
import PopupType from 'enums/PopupType';
import TreeStore from 'stores/TreeStore';
import TextStore from 'stores/TextStore';
import Texts from 'constants/Texts';
import TreeStoreType from 'enums/TreeStoreType';

interface Props {
    qpIntegrationStore?: QpIntegrationStore;
    treeStore?: TreeStore;
    popupStore?: PopupStore;
    textStore?: TextStore;
}

interface State {
    discriminator: DiscriminatorModel;
    version: VersionType;
}

@inject('qpIntegrationStore', 'treeStore', 'popupStore', 'textStore')
@observer
export default class AddVersionPopup extends React.Component<Props, State> {

    state = {
        discriminator: null as DiscriminatorModel,
        version: VersionType.Content,
    };

    private addClick = () => {
        const { popupStore, qpIntegrationStore, treeStore } = this.props;
        const { discriminator, version } = this.state;
        const node = treeStore.getTreeStore(TreeStoreType.SITE).selectedNode as PageModel;
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
        const { popupStore, textStore } = this.props;
        const { version } = this.state;

        if (popupStore.type !== PopupType.ADDVERSION) {
            return null;
        }

        let useOnlyContentVersion = false;
        if (popupStore.options && popupStore.options.onlyContentVersion) {
            useOnlyContentVersion = true;
        }

        return (
            <Card>
                <FormGroup>
                    <RadioGroup
                        label={textStore.texts[Texts.popupFieldVersion]}
                        selectedValue={version}
                        onChange={this.changeVersion}
                    >
                        <Radio
                            label={textStore.texts[Texts.popupVersionContent]}
                            value={VersionType.Content}
                            disabled={useOnlyContentVersion}
                        />
                        <Radio
                            label={textStore.texts[Texts.popupVersionStructural]}
                            value={VersionType.Structural}
                            disabled={useOnlyContentVersion}
                        />
                    </RadioGroup>
                </FormGroup>
                <FormGroup>
                    <DiscriminatorSelect items={popupStore.discriminators} onChange={this.changeDiscriminator}/>
                </FormGroup>
                <ButtonGroup className="dialog-button-group">
                    <Button text={textStore.texts[Texts.popupAddButton]} icon="add" onClick={this.addClick} intent={Intent.SUCCESS}/>
                    <Button text={textStore.texts[Texts.popupCancelButton]} icon="undo" onClick={this.cancelClick}/>
                </ButtonGroup>
            </Card>
        );
    }
}
