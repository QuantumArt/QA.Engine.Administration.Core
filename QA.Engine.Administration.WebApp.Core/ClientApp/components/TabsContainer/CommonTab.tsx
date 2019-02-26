import * as React from 'react';
import { observer, inject } from 'mobx-react';
import { Button, Navbar, NavbarGroup, H5, Intent, Spinner, InputGroup, Checkbox } from '@blueprintjs/core';
import ExtensionCard from './ExtensionCard';
import OperationState from 'enums/OperationState';
import NavigationStore from 'stores/NavigationStore';
import SiteTreeStore from 'stores/TreeStore/SiteTreeStore';
import TreeStore from 'stores/TreeStore';
import QpIntegrationStore from 'stores/QpIntegrationStore';
import EditArticleStore from 'stores/EditArticleStore';
import TextStore from 'stores/TextStore';
import Texts from 'constants/Texts';

interface Props {
    navigationStore?: NavigationStore;
    qpIntegrationStore?: QpIntegrationStore;
    editArticleStore?: EditArticleStore;
    treeStore?: TreeStore;
    textStore?: TextStore;
}

@inject('navigationStore', 'qpIntegrationStore', 'editArticleStore', 'treeStore', 'textStore')
@observer
export default class CommonTab extends React.Component<Props> {

    private refreshClick = () => {
        this.props.treeStore.updateSubTree();
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
            isVisible: editArticleStore.isVisible,
            isInSiteMap: editArticleStore.isInSiteMap,
            extensionId: tree.selectedNode.extensionId,
            fields: editArticleStore.changedFields,
        };
        if (tree instanceof SiteTreeStore) {
            tree.edit(model).then(() => editArticleStore.init(tree.selectedNode));
        }
    }

    private changeTitle = (e: React.ChangeEvent<HTMLInputElement>) => {
        const { editArticleStore } = this.props;
        editArticleStore.setTitle(e.target.value);
    }

    private changeIsVisible = (e: React.ChangeEvent<HTMLInputElement>) => {
        const { editArticleStore } = this.props;
        editArticleStore.setIsVisible(e.target.checked);
    }

    private changeIsInSiteMap = (e: React.ChangeEvent<HTMLInputElement>) => {
        const { editArticleStore } = this.props;
        editArticleStore.setIsInSiteMap(e.target.checked);
    }

    render() {
        const { treeStore, textStore, editArticleStore: { title, isVisible, isInSiteMap, isEditable } } = this.props;
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
                            <Button minimal icon="refresh" text={textStore.texts[Texts.refresh]} onClick={this.refreshClick}/>
                            {isEditable ? [
                                <Button key={1} minimal icon="edit" text={textStore.texts[Texts.edit]} intent={Intent.PRIMARY} onClick={this.editClick} />,
                                <Button key={2} minimal icon="saved" text={textStore.texts[Texts.save]} intent={Intent.SUCCESS} onClick={this.saveClick} />,
                            ] : null}
                        </NavbarGroup>
                    </Navbar>
                    <div className="tab-content">
                        <div className="tab-entity">
                            <H5>ID</H5>
                            <p>{selectedNode.id}</p>
                        </div>
                        <div className="tab-entity">
                            <H5>{textStore.texts[Texts.title]}</H5>
                            {isEditable ? (
                                <InputGroup value={title} onChange={this.changeTitle} />
                                ) : (
                                <p>{title}</p>
                            )}
                        </div>
                        <div className="tab-entity">
                            <H5>{textStore.texts[Texts.typeName]}</H5>
                            <p>{selectedNode.discriminatorTitle}</p>
                        </div>
                        <div className="tab-entity">
                            <H5>{textStore.texts[Texts.alias]}</H5>
                            <p>{selectedNode.alias}</p>
                        </div>
                        <div className="tab-entity">
                            <Checkbox checked={selectedNode.published} disabled={true}>{textStore.texts[Texts.published]}</Checkbox>
                        </div>
                        <div className="tab-entity">
                            <Checkbox checked={isInSiteMap} onChange={this.changeIsInSiteMap} disabled={!isEditable}>{textStore.texts[Texts.isInSiteMap]}</Checkbox>
                        </div>
                        <div className="tab-entity">
                            <Checkbox checked={isVisible} onChange={this.changeIsVisible} disabled={!isEditable}>{textStore.texts[Texts.isVisible]}</Checkbox>
                        </div>
                        {isEditable ? (<ExtensionCard />) : null}
                    </div>
                </div>
            );
        }

        return null;
    }
}
