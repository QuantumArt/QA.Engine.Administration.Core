import * as React from 'react';
import lodashThrottle from 'lodash.throttle';
import TreeStore from 'stores/TreeStore';
import { Card, ButtonGroup, Button, Intent } from '@blueprintjs/core';
import PopupStore from 'stores/PopupStore';
import PopupType from 'enums/PopupType';
import TextStore from 'stores/TextStore';
import Texts from 'constants/Texts';
import { inject, observer } from 'mobx-react';
import TreeStructure from 'components/TreeStructure';
import { ITreeElement } from 'stores/TreeStore/BaseTreeStore';

interface Props {
    treeStore?: TreeStore;
    popupStore?: PopupStore;
    textStore?: TextStore;
}

interface State {
    newParent: ITreeElement;
    height: number;
}

@inject('treeStore', 'popupStore', 'textStore')
@observer
export default class MovePopup extends React.Component<Props, State> {

    state = {
        newParent: null as ITreeElement,
        height: 500,
    };

    private moveClick = () => {
        const { treeStore, popupStore } = this.props;
        const { newParent } = this.state;
        const model: MoveModel = {
            newParentId: +newParent.id,
            itemId: popupStore.itemId,
        };
        treeStore.move(model);
        popupStore.close();
    }

    private changeNewParent = (e: ITreeElement) => {
        const { treeStore } = this.props;
        const moveTreeStore = treeStore.getMoveTreeStore();
        this.setState({ newParent: moveTreeStore.selectedNode == null ? null : e });
    }

    private cancelClick = () => this.props.popupStore.close();

    private handleResize = lodashThrottle(
        () => {
            this.setState({
                height: window.innerHeight - 300,
            });
        },
        100,
    );

    componentDidMount() {
        this.handleResize();
        window.addEventListener('resize', this.handleResize);
    }

    componentWillUnmount() {
        window.removeEventListener('resize', this.handleResize);
    }

    render() {
        const { popupStore, textStore, treeStore } = this.props;

        if (popupStore.type !== PopupType.MOVE) {
            return null;
        }

        const moveTreeStore = treeStore.getMoveTreeStore();
        const { newParent } = this.state;

        return (
            <Card style={{ height: this.state.height }}>
                <TreeStructure
                    type="move"
                    className="popup-tree-pane"
                    tree={moveTreeStore}
                    sbHeightDelta={499}
                    onNodeClick={this.changeNewParent}
                />
                <ButtonGroup className="dialog-button-group">
                    <Button text={textStore.texts[Texts.popupMoveButton]} icon="move" onClick={this.moveClick} intent={Intent.SUCCESS} disabled={newParent == null} />
                    <Button text={textStore.texts[Texts.popupCancelButton]} icon="undo" onClick={this.cancelClick} />
                </ButtonGroup>
            </Card>
        );
    }
}
