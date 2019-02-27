import * as React from 'react';
import { Button, ButtonGroup, Card, Checkbox, FormGroup, Intent, Radio, RadioGroup } from '@blueprintjs/core';
import { inject, observer } from 'mobx-react';
import PopupStore from 'stores/PopupStore';
import PageSelect from 'components/Select/PageSelect';
import PopupType from 'enums/PopupType';
import TreeStore from 'stores/TreeStore';
import SiteTreeStore from 'stores/TreeStore/SiteTreeStore';
import TextStore from 'stores/TextStore';
import Texts from 'constants/Texts';

interface Props {
    treeStore?: TreeStore;
    popupStore?: PopupStore;
    textStore?: TextStore;
}

interface State {
    deleteAllVersions: boolean;
    deleteContentVersions: ContentVersionOperations;
    contentVersionId: number;
}

enum ContentVersionOperations {
    archive = 'archive',
    move = 'move',
}

@inject('treeStore', 'popupStore', 'textStore')
@observer
export default class ArchivePopup extends React.Component<Props, State> {

    state = {
        deleteAllVersions: false,
        deleteContentVersions: ContentVersionOperations.archive,
        contentVersionId: null as number,
    };

    private async archiveWidget(treeStore: TreeStore, model: RemoveModel): Promise<void> {
        await treeStore.getWidgetStore().archive(model);
        await treeStore.updateSubTree();
    }

    private async archiveContentVersion(treeStore: TreeStore, model: RemoveModel): Promise<void> {
        await treeStore.getContentVersionsStore().archive(model);
        await treeStore.updateSubTree();
    }

    private archiveClick = () => {
        const { popupStore, treeStore } = this.props;
        const { deleteAllVersions, deleteContentVersions, contentVersionId } = this.state;
        const model: RemoveModel = {
            contentVersionId,
            itemId: popupStore.itemId,
            isDeleteAllVersions: deleteAllVersions,
            isDeleteContentVersions: deleteContentVersions === ContentVersionOperations.archive,
        };
        if (popupStore.type === PopupType.ARCHIVE) {
            (treeStore.resolveTreeStore() as SiteTreeStore).archive(model);
        }
        if (popupStore.type === PopupType.ARCHIVEWIDGET) {
            this.archiveWidget(treeStore, model);
        }
        if (popupStore.type === PopupType.ARCHIVECONTENTVERSION) {
            this.archiveContentVersion(treeStore, model);
        }
        popupStore.close();
    }

    private cancelClick = () => this.props.popupStore.close();

    private changeContentVersion = (e: PageModel) =>
        this.setState({ contentVersionId: e.id })

    private changeDeleteAllVersions = (version: React.ChangeEvent<HTMLInputElement>) =>
        this.setState({ deleteAllVersions: version.target.checked })

    private changeDeleteContentVersions = (version: React.ChangeEvent<HTMLInputElement>) =>
        this.setState({ deleteContentVersions: version.target.value as ContentVersionOperations })

    render() {
        const { popupStore, textStore } = this.props;
        const { deleteAllVersions, deleteContentVersions } = this.state;

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
                            value={ContentVersionOperations.archive}
                        />
                        <Radio
                            label={textStore.texts[Texts.popupMoveVersion]}
                            value={ContentVersionOperations.move}
                        />
                    </RadioGroup>
                </FormGroup>
                <FormGroup>
                    <PageSelect
                        items={popupStore.contentVersions}
                        onChange={this.changeContentVersion}
                        disabled={deleteAllVersions || deleteContentVersions === ContentVersionOperations.archive}
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
