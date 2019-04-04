import * as React from 'react';
import { Button, ButtonGroup, Card, Checkbox, FormGroup, Intent, Radio, RadioGroup } from '@blueprintjs/core';
import { inject, observer } from 'mobx-react';
import PopupStore from 'stores/PopupStore';
import PageSelect from 'components/Select/PageSelect';
import PopupType from 'enums/PopupType';
import TreeStore from 'stores/TreeStore';
import TextStore from 'stores/TextStore';
import Texts from 'constants/Texts';

type ContentVersionOperations = 'archive' | 'move';

interface Props {
    treeStore?: TreeStore;
    popupStore?: PopupStore;
    textStore?: TextStore;
}

interface State {
    deleteAllVersions: boolean;
    deleteContentVersions: ContentVersionOperations;
    contentVersion: PageModel;
    contentVersionIntent: Intent;
}

@inject('treeStore', 'popupStore', 'textStore')
@observer
export default class ArchivePopup extends React.Component<Props, State> {

    private resetIntent = { contentVersionIntent: Intent.NONE };
    state = {
        deleteAllVersions: false,
        deleteContentVersions: 'archive' as ContentVersionOperations,
        contentVersion: null as PageModel,
        ...this.resetIntent,
    };

    private archiveClick = () => {
        const { popupStore, treeStore } = this.props;
        const { deleteAllVersions, deleteContentVersions, contentVersion } = this.state;
        if (!deleteAllVersions && deleteContentVersions === 'move' && contentVersion == null) {
            this.setState({ contentVersionIntent: Intent.DANGER });
            return;
        }
        const model: RemoveModel = {
            itemId: popupStore.itemId,
            isDeleteAllVersions: deleteAllVersions,
            isDeleteContentVersions: deleteAllVersions ? true : deleteContentVersions === 'archive',
            contentVersionId: (contentVersion == null || deleteContentVersions === 'archive' || deleteAllVersions) ? null : contentVersion.id,
        };
        treeStore.archive(model);
        popupStore.close();
    }

    private cancelClick = () => this.props.popupStore.close();

    private changeContentVersion = (e: PageModel) =>
        this.setState({ contentVersion: e, ...this.resetIntent })

    private changeDeleteAllVersions = (version: React.ChangeEvent<HTMLInputElement>) =>
        this.setState({ deleteAllVersions: version.target.checked, contentVersion: null, ...this.resetIntent })

    private changeDeleteContentVersions = (version: React.ChangeEvent<HTMLInputElement>) =>
        this.setState({ deleteContentVersions: version.target.value as ContentVersionOperations, contentVersion: null, ...this.resetIntent })

    render() {
        const { popupStore, textStore } = this.props;
        const { deleteAllVersions, deleteContentVersions, contentVersion, contentVersionIntent } = this.state;

        if ([PopupType.ARCHIVE, PopupType.ARCHIVEWIDGET, PopupType.ARCHIVECONTENTVERSION].indexOf(popupStore.type) < 0) {
            return null;
        }

        if ([PopupType.ARCHIVEWIDGET, PopupType.ARCHIVECONTENTVERSION].indexOf(popupStore.type) > -1) {
            return (
                <Card>
                    <ButtonGroup className="dialog-button-group">
                        <Button text={textStore.texts[Texts.popupArchiveButton]} icon="box" onClick={this.archiveClick} intent={Intent.DANGER} />
                        <Button text={textStore.texts[Texts.popupCancelButton]} icon="undo" onClick={this.cancelClick} />
                    </ButtonGroup>
                </Card>
            );
        }

        return (
            <Card>
                <FormGroup>
                    <Checkbox checked={deleteAllVersions} onChange={this.changeDeleteAllVersions}>
                        {textStore.texts[Texts.popupArchiveAllVersion]}
                    </Checkbox>
                </FormGroup>
                <FormGroup>
                    <RadioGroup
                        label={textStore.texts[Texts.popupActionType]}
                        inline={true}
                        selectedValue={deleteContentVersions}
                        onChange={this.changeDeleteContentVersions}
                        disabled={deleteAllVersions}
                    >
                        <Radio
                            label={textStore.texts[Texts.popupArchive]}
                            value={'archive'}
                        />
                        <Radio
                            label={textStore.texts[Texts.popupMoveVersion]}
                            value={'move'}
                        />
                    </RadioGroup>
                </FormGroup>
                <FormGroup>
                    <PageSelect
                        items={popupStore.contentVersions}
                        onChange={this.changeContentVersion}
                        disabled={deleteAllVersions || deleteContentVersions === 'archive'}
                        intent={contentVersionIntent}
                    />
                </FormGroup>
                <ButtonGroup className="dialog-button-group">
                    <Button text={textStore.texts[Texts.popupArchiveButton]} icon="box" onClick={this.archiveClick} intent={Intent.DANGER} />
                    <Button text={textStore.texts[Texts.popupCancelButton]} icon="undo" onClick={this.cancelClick} />
                </ButtonGroup>
            </Card>
        );
    }
}
