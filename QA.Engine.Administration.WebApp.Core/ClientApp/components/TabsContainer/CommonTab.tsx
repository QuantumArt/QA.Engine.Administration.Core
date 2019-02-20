import * as React from 'react';
import { observer, inject } from 'mobx-react';
import { Button, Navbar, NavbarGroup, H5, Intent, Spinner, InputGroup } from '@blueprintjs/core';
import ExtantionCard from './ExtensionCard';
import OperationState from 'enums/OperationState';
import { NavigationState } from 'stores/NavigationStore';
import { SiteTreeState } from 'stores/SiteTreeStore';
import { QpIntegrationState } from 'stores/QpIntegrationStore';
import { EditArticleState } from 'stores/EditArticleStore';
import { observable } from 'mobx';

interface Props {
    navigationStore?: NavigationState;
    qpIntegrationStore?: QpIntegrationState;
    editArticleStore?: EditArticleState;
}

@inject('navigationStore', 'qpIntegrationStore', 'editArticleStore')
@observer
export default class CommonTab extends React.Component<Props> {

    private refreshClick = () => {
        const { navigationStore } = this.props;
        const treeStore = navigationStore.resolveTreeStore();
        treeStore.updateSubTree(treeStore.selectedNode.id);
    }

    private editClick = () => {
        const { navigationStore, qpIntegrationStore } = this.props;
        const treeStore = navigationStore.resolveTreeStore();
        qpIntegrationStore.edit(treeStore.selectedNode.id);
    }

    private saveClick = () => {
        const { navigationStore, editArticleStore } = this.props;
        const treeStore = navigationStore.resolveTreeStore();
        const model: EditModel = {
            itemId: treeStore.selectedNode.id,
            title: editArticleStore.title,
            extensionId: treeStore.selectedNode.extensionId,
            fields: editArticleStore.changedFields,
        };
        if (treeStore instanceof SiteTreeState) {
            treeStore as SiteTreeState;
            treeStore.edit(model);
        }
    }

    private changeTitle = (e: React.ChangeEvent<HTMLInputElement>) => {
        const { editArticleStore } = this.props;
        editArticleStore.setTitle(e.target.value);
    }

    render() {
        const { navigationStore, editArticleStore } = this.props;
        const title = editArticleStore.title;
        const treeStore = navigationStore.resolveTreeStore();
        if (treeStore.selectedNode == null) {
            return null;
        }
        if (treeStore.treeState === OperationState.NONE || treeStore.treeState === OperationState.PENDING) {
            return (<Spinner size={60} />);
        }
        const selectedNode = treeStore.selectedNode;
        if (selectedNode != null) {
            return (
                <div className="tab">
                    <Navbar className="tab-navbar">
                        <NavbarGroup>
                            <Button minimal icon="refresh" text="Refresh" onClick={this.refreshClick}/>
                            <Button minimal icon="edit" text="Edit" intent={Intent.PRIMARY} onClick={this.editClick} />
                            <Button minimal icon="saved" text="Save" intent={Intent.SUCCESS} onClick={this.saveClick} />
                        </NavbarGroup>
                    </Navbar>
                    <div className="tab-content">
                        <div className="tab-entity">
                            <H5>ID</H5>
                            <p>{selectedNode.id}</p>
                        </div>
                        <div className="tab-entity">
                            <H5>Title</H5>
                            <InputGroup value={title} onChange={this.changeTitle} />
                        </div>
                        <div className="tab-entity">
                            <H5>Type Name</H5>
                            <p>{selectedNode.discriminatorTitle}</p>
                        </div>
                        <div className="tab-entity">
                            <H5>Alias</H5>
                            <p>{selectedNode.alias}</p>
                        </div>
                        <div className="tab-entity">
                            <H5>Status</H5>
                            <p>{selectedNode.published ? 'Published' : 'Not published'}</p>
                        </div>
                        <div className="tab-entity">
                            <H5>View in the site map</H5>
                            <p>{selectedNode.isInSiteMap ? 'Yes' : 'No'}</p>
                        </div>
                        <div className="tab-entity">
                            <H5>Visible</H5>
                            <p>{selectedNode.isVisible ? 'Yes' : 'No'}</p>
                        </div>
                        <ExtantionCard />
                    </div>
                </div>
            );
        }

        return null;
    }
}
