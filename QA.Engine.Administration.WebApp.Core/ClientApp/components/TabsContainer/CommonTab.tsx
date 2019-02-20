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
import TreeStore from 'stores/TreeStore';

interface Props {
    navigationStore?: NavigationState;
    qpIntegrationStore?: QpIntegrationState;
    editArticleStore?: EditArticleState;
    treeStore?: TreeStore;
}

@inject('navigationStore', 'qpIntegrationStore', 'editArticleStore', 'treeStore')
@observer
export default class CommonTab extends React.Component<Props> {

    private refreshClick = () => {
        const { treeStore } = this.props;
        const tree = treeStore.resolveTreeStore();
        tree.updateSubTree(tree.selectedNode.id);
    }

    private editClick = () => {
        const { treeStore, qpIntegrationStore } = this.props;
        const tree = treeStore.resolveTreeStore();
        qpIntegrationStore.edit(tree.selectedNode.id);
    }

    private saveClick = () => {
        const { treeStore, editArticleStore } = this.props;
        const tree = treeStore.resolveTreeStore();
        const model: EditModel = {
            itemId: tree.selectedNode.id,
            title: editArticleStore.title,
            extensionId: tree.selectedNode.extensionId,
            fields: editArticleStore.changedFields,
        };
        if (tree instanceof SiteTreeState) {
            tree as SiteTreeState;
            tree.edit(model);
        }
    }

    private changeTitle = (e: React.ChangeEvent<HTMLInputElement>) => {
        const { editArticleStore } = this.props;
        editArticleStore.setTitle(e.target.value);
    }

    render() {
        const { treeStore, editArticleStore } = this.props;
        const title = editArticleStore.title;
        const tree = treeStore.resolveTreeStore();
        if (tree.selectedNode == null) {
            return null;
        }
        if (tree.treeState === OperationState.NONE || tree.treeState === OperationState.PENDING) {
            return (<Spinner size={60} />);
        }
        const selectedNode = tree.selectedNode;
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
