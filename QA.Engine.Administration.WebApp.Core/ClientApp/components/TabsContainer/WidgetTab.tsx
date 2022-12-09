import * as React from 'react';
import { inject, observer } from 'mobx-react';
import PopupStore from 'stores/PopupStore';
import TreeStore from 'stores/TreeStore';
import PopupType from 'enums/PopupType';
import OperationState from 'enums/OperationState';
import { Button, Intent, Navbar, NavbarGroup, Spinner } from '@blueprintjs/core';
import TreeStructure from 'components/TreeStructure';
import InfoPane from './InfoPane';
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
        const tree = treeStore.getWidgetTreeStore();
        popupStore.show(tree.selectedSiteTreeNode.id, PopupType.ADDWIDGET, textStore.texts[Texts.popupAddWidgetTitle]);
    }

    private refreshClick = () => {
        this.props.treeStore.updateSubTree();
    }

    render() {
        const { treeStore, textStore } = this.props;
        const tree = treeStore.getWidgetTreeStore();

        if (tree.selectedSiteTreeNode == null) {
            return null;
        }

        if (treeStore.state === OperationState.PENDING) {
            return <Spinner size={60}/>;
        }

        return (
            <div className="tab">
                <Navbar className="tab-navbar">
                    <NavbarGroup>
                        <Button
                            minimal
                            icon="refresh"
                            text={textStore.texts[Texts.refresh]}
                            onClick={this.refreshClick}
                        />
                        <Button
                            minimal
                            icon="add"
                            text={textStore.texts[Texts.add]}
                            intent={Intent.PRIMARY}
                            onClick={this.addClick}/>
                    </NavbarGroup>
                </Navbar>
                <div className="tab-content tab-content--row">
                    <TreeStructure
                        type="widgets"
                        className="minor-tree-pane"
                        tree={tree}
                        sbHeightDelta={245}
                    />
                    <InfoPane type="widgets"/>
                </div>
            </div>
        );
    }
}
