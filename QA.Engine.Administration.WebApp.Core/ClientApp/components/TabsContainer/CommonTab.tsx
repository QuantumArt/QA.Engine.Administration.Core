import * as React from 'react';
import { observer, inject } from 'mobx-react';
import { Button, Navbar, NavbarGroup, H5, Intent, Spinner, InputGroup, Checkbox } from '@blueprintjs/core';
import OperationState from 'enums/OperationState';
import SiteTreeStore from 'stores/TreeStore/SiteTreeStore';
import TreeStore from 'stores/TreeStore';
import QpIntegrationStore from 'stores/QpIntegrationStore';
import EditArticleStore from 'stores/EditArticleStore';
import TextStore from 'stores/TextStore';
import Texts from 'constants/Texts';
import ExtensionCard from './ExtensionCard';

interface Props {
    qpIntegrationStore?: QpIntegrationStore;
    editArticleStore?: EditArticleStore;
    treeStore?: TreeStore;
    textStore?: TextStore;
}

@inject('qpIntegrationStore', 'editArticleStore', 'treeStore', 'textStore')
@observer
export default class CommonTab extends React.Component<Props> {

    private refreshClick = () => {
        this.props.treeStore.updateSubTree();
    }

    private editClick = () => {
        const { treeStore, qpIntegrationStore } = this.props;
        const tree = treeStore.getSiteTreeStore();
        qpIntegrationStore.edit(tree.selectedNode.id);
    }

    private saveClick = () => {
        const { treeStore, editArticleStore } = this.props;
        const tree = treeStore.getSiteTreeStore();
        const model: EditModel = {
            itemId: tree.selectedNode.id,
            title: editArticleStore.title,
            isInSiteMap: editArticleStore.isInSiteMap,
        };
        if (tree instanceof SiteTreeStore) {
            (async () => {
                await treeStore.edit(model);
                await editArticleStore.init(tree.selectedNode);
            })();
        }
    }

    private changeTitle = (e: React.ChangeEvent<HTMLInputElement>) => {
        const { editArticleStore } = this.props;
        editArticleStore.setTitle(e.target.value);
    }

    private changeIsInSiteMap = (e: React.ChangeEvent<HTMLInputElement>) => {
        const { editArticleStore } = this.props;
        editArticleStore.setIsInSiteMap(e.target.checked);
    }

    render() {
        const {
            treeStore,
            textStore,
            editArticleStore: { title, isInSiteMap, isEditable },
        } = this.props;
        const tree = treeStore.resolveMainTreeStore();

        if (tree.selectedNode == null) {
            return null;
        }
        if (treeStore.state === OperationState.PENDING) {
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
                            <Checkbox checked={selectedNode.published} disabled>{textStore.texts[Texts.published]}</Checkbox>
                        </div>
                        <div className="tab-entity">
                            <Checkbox checked={isInSiteMap} onChange={this.changeIsInSiteMap} disabled={!isEditable}>{textStore.texts[Texts.isInSiteMap]}</Checkbox>
                        </div>
                        <div className="tab-entity">
                            <Checkbox checked={selectedNode.visible} disabled>{textStore.texts[Texts.visible]}</Checkbox>
                        </div>
                        {isEditable && <ExtensionCard />}
                    </div>
                </div>
            );
        }

        return null;
    }
}
