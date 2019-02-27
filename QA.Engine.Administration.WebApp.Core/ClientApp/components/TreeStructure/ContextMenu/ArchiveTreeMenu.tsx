import * as React from 'react';
import { Intent, Menu, MenuDivider, MenuItem } from '@blueprintjs/core';
import { observer, inject } from 'mobx-react';
import PopupStore from 'stores/PopupStore';
import PopupType from 'enums/PopupType';
import { ITreeElement } from 'stores/TreeStore/BaseTreeStore';
import TreeStore from 'stores/TreeStore';
import TextStore from 'stores/TextStore';
import Texts from 'constants/Texts';

interface Props {
    popupStore?: PopupStore;
    treeStore?: TreeStore;
    textStore?: TextStore;
    itemId: number;
    node: ITreeElement;
}

@inject('popupStore', 'treeStore', 'textStore')
@observer
export default class ArchiveTreeMenu extends React.Component<Props> {

    private updateClick = () => {
        this.props.treeStore.updateSubTree();
    }

    private restoreClick = () => {
        const { popupStore, itemId, textStore } = this.props;
        popupStore.show(itemId, PopupType.RESTORE, textStore.texts[Texts.popupRestoreItemTitle]);
    }

    private deleteClick = () => {
        const { popupStore, itemId, textStore } = this.props;
        popupStore.show(itemId, PopupType.DELETE, textStore.texts[Texts.popupDeleteItemTitle]);
    }

    private handleClick = (e: React.MouseEvent<HTMLElement>, cb: () => void) => {
        e.stopPropagation();
        this.props.node.isContextMenuActive = false;
        cb();
    }

    render() {
        const { textStore } = this.props;
        return (
            <Menu>
                <MenuItem
                    onClick={(e: React.MouseEvent<HTMLElement>) => this.handleClick(e, this.updateClick)}
                    icon="refresh"
                    text={textStore.texts[Texts.refresh]}
                />
                <MenuItem
                    onClick={(e: React.MouseEvent<HTMLElement>) => this.handleClick(e, this.restoreClick)}
                    icon="swap-horizontal"
                    text={textStore.texts[Texts.restore]}
                />
                <MenuDivider/>
                <MenuItem
                    onClick={(e: React.MouseEvent<HTMLElement>) => this.handleClick(e, this.deleteClick)}
                    icon="delete"
                    text={textStore.texts[Texts.delete]}
                    intent={Intent.DANGER}/>
            </Menu>
        );
    }
}
