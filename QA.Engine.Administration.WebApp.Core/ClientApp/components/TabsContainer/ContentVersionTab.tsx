import * as React from 'react';
import { observer, inject } from 'mobx-react';
import { Button, Navbar, NavbarGroup, H5, Intent, Spinner, InputGroup, Card, Checkbox } from '@blueprintjs/core';
import OperationState from 'enums/OperationState';
import PopupStore from 'stores/PopupStore';
import TreeStore from 'stores/TreeStore';
import ContentVersionTree from 'components/SiteTree/ContentVersionTree';
import PopupType from 'enums/PopupType';

interface Props {
    popupStore?: PopupStore;
    treeStore?: TreeStore;
}

@inject('popupStore', 'treeStore')
@observer
export default class ContentVersionTab extends React.Component<Props> {

    private addClick = () => {
        const { treeStore, popupStore } = this.props;
        const tree = treeStore.resolveTreeStore();
        popupStore.show(tree.selectedNode.id, PopupType.ADDVERSION, 'Добавить раздел');
    }

    private refreshClick = () => {
        this.props.treeStore.updateSubTree();
    }

    render() {

        const { treeStore } = this.props;
        const tree = treeStore.getContentVersionsStore();
        const selectedNode = tree.selectedNode;

        if (tree.treeState === OperationState.NONE || tree.treeState === OperationState.PENDING) {
            return (<Spinner size={60} />);
        }

        const tab = selectedNode == null ? null : (
            <Card>
                <div className="tab-content">
                    <div className="tab-entity">
                        <H5>ID</H5>
                        <p>{selectedNode.id}</p>
                    </div>
                    <div className="tab-entity">
                        <H5>Title</H5>
                        <p>{selectedNode.title}</p>
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
                        <Checkbox checked={selectedNode.published} disabled={true}>Published</Checkbox>
                    </div>
                    <div className="tab-entity">
                        <Checkbox checked={selectedNode.isVisible} disabled={true}>Visible</Checkbox>
                    </div>
                </div>
            </Card>
        );

        return (
            <div className="tab">
                <Navbar className="tab-navbar">
                    <NavbarGroup>
                        <Button minimal icon="refresh" text="Refresh" onClick={this.refreshClick}/>
                        <Button minimal icon="add" text="Add" intent={Intent.PRIMARY} onClick={this.addClick} />
                        {/* <Button minimal icon="saved" text="Save" intent={Intent.SUCCESS} onClick={this.saveClick} /> */}
                    </NavbarGroup>
                </Navbar>
                <ContentVersionTree />
                {tab}
            </div>
        );
    }
}
