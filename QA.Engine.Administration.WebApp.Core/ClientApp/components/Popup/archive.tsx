import * as React from 'react';
import { Card, Spinner, Button, Checkbox, RadioGroup, Radio } from '@blueprintjs/core';
import { observer, inject } from 'mobx-react';
import { PopupState } from 'stores/PopupStore';
import OperationState from 'enums/OperationState';
import PageSelect from 'components/Select/PageSelect';
import PopupType from 'enums/PopupType';
import TreeStore from 'stores/TreeStore';
import { SiteTreeState } from 'stores/SiteTreeStore';

interface Props {
    treeStore?: TreeStore;
    popupStore?: PopupState;
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
        (treeStore.resolveTreeStore() as SiteTreeState).archive(model);
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

        if (popupStore.state === OperationState.NONE || popupStore.state === OperationState.PENDING) {
            return (<Spinner size={30} />);
        }

        return (
            <Card>
                <Checkbox
                    checked={deleteAllVersions}
                    onChange={this.changeDeleteAllVersions}
                >
                    Archive versions
                </Checkbox>
                <RadioGroup
                    label="Action on content versions"
                    inline={true}
                    selectedValue={deleteContentVersions}
                    onChange={this.changeDeleteContentVersions}
                    disabled={deleteAllVersions}
                >
                    <Radio
                        label="Archive content versions"
                        value={ContentVersionOperations.archive}
                    />
                    <Radio
                        label="Move to another version"
                        value={ContentVersionOperations.move}
                    />
                </RadioGroup>
                <div>
                    <PageSelect
                        items={popupStore.contentVersions}
                        onChange={this.changeContentVersion}
                        disabled={deleteAllVersions || deleteContentVersions === ContentVersionOperations.archive}
                    />
                </div>
                <div>
                    <Button text="archive" onClick={this.archiveClick} />
                    <Button text="cancel" onClick={this.cancelClick} />
                </div>
            </Card>
        );
    }
}
