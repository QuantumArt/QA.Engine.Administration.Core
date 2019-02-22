import * as React from 'react';
import { Button, ButtonGroup, Card, Checkbox, FormGroup, Intent, Radio, RadioGroup } from '@blueprintjs/core';
import { inject, observer } from 'mobx-react';
import PopupStore from 'stores/PopupStore';
import PageSelect from 'components/Select/PageSelect';
import PopupType from 'enums/PopupType';
import TreeStore from 'stores/TreeStore';
import SiteTreeStore from 'stores/TreeStore/SiteTreeStore';

interface Props {
    treeStore?: TreeStore;
    popupStore?: PopupStore;
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

@inject('treeStore', 'popupStore')
@observer
export default class ArchivePopup extends React.Component<Props, State> {

    state = {
        deleteAllVersions: false,
        deleteContentVersions: ContentVersionOperations.archive,
        contentVersionId: null as number,
    };

    private archiveClick = () => {
        const { popupStore, treeStore } = this.props;
        const { deleteAllVersions, deleteContentVersions, contentVersionId } = this.state;
        const model: RemoveModel = {
            contentVersionId,
            itemId: popupStore.itemId,
            isDeleteAllVersions: deleteAllVersions,
            isDeleteContentVersions: deleteContentVersions === ContentVersionOperations.archive,
        };
        (treeStore.resolveTreeStore() as SiteTreeStore).archive(model);
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
        const { popupStore } = this.props;
        const { deleteAllVersions, deleteContentVersions } = this.state;
        if (popupStore.type !== PopupType.ARCHIVE) {
            return null;
        }

        return (
            <Card>
                <FormGroup>
                    <Checkbox checked={deleteAllVersions} onChange={this.changeDeleteAllVersions}>
                        Архивировать все версии
                    </Checkbox>
                </FormGroup>
                <FormGroup>
                    <RadioGroup
                        label="Тип действия"
                        inline={true}
                        selectedValue={deleteContentVersions}
                        onChange={this.changeDeleteContentVersions}
                        disabled={deleteAllVersions}
                    >
                        <Radio
                            label="Архивировать"
                            value={ContentVersionOperations.archive}
                        />
                        <Radio
                            label="Сменить версию"
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
                    <Button text="Архивировать" icon="box" onClick={this.archiveClick} intent={Intent.DANGER} />
                    <Button text="Отмена" icon="undo" onClick={this.cancelClick} />
                </ButtonGroup>
            </Card>
        );
    }
}