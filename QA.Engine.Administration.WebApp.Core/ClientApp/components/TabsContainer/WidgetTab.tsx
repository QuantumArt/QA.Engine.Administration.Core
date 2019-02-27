import * as React from 'react';
import { observer, inject } from 'mobx-react';
import PopupStore from 'stores/PopupStore';
import TreeStore from 'stores/TreeStore';
import PopupType from 'enums/PopupType';
import OperationState from 'enums/OperationState';
import { Spinner, Card, H5, Navbar, NavbarGroup, Button, Intent, Checkbox } from '@blueprintjs/core';
import WidgetTree from 'components/SiteTree/WidgetTree';
import TextStore from 'stores/TextStore';
import Texts from 'constants/Texts';

interface Props {
    popupStore?: PopupStore;
    treeStore?: TreeStore;
    textStore?: TextStore;
}

@inject('popupStore', 'treeStore', 'textStore')
@observer
export default class WidgetTab extends React.Component<Props> {

    private addClick = () => {
        const { treeStore, popupStore, textStore } = this.props;
        const tree = treeStore.resolveTreeStore();
        popupStore.show(tree.selectedNode.id, PopupType.ADDWIDGET, textStore.texts[Texts.popupAddWidgetTitle]);
    }

    private refreshClick = () => {
        this.props.treeStore.updateSubTree();
    }

    render() {

        const { treeStore, textStore } = this.props;
        const tree = treeStore.getWidgetStore();
        const siteTree = treeStore.resolveTreeStore();
        const selectedNode = tree.selectedNode;

        if (tree.selectedSiteTreeNode == null) {
            return null;
        }

        const loadingStates = [OperationState.NONE, OperationState.PENDING];
        if (loadingStates.indexOf(tree.treeState) > -1 || loadingStates.indexOf(siteTree.treeState) > -1) {
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
                        <H5>{textStore.texts[Texts.title]}</H5>
                        <p>{selectedNode.title}</p>
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
                        <Checkbox checked={selectedNode.isVisible} disabled={true}>{textStore.texts[Texts.isVisible]}</Checkbox>
                    </div>
                </div>
            </Card>
        );

        return (
            <div className="tab">
                <Navbar className="tab-navbar">
                    <NavbarGroup>
                        <Button minimal icon="refresh" text={textStore.texts[Texts.refresh]} onClick={this.refreshClick}/>
                        <Button minimal icon="add" text={textStore.texts[Texts.add]} intent={Intent.PRIMARY} onClick={this.addClick} />
                    </NavbarGroup>
                </Navbar>
                <WidgetTree />
                {tab}
            </div>
        );
    }
}
