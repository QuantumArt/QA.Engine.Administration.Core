import * as React from 'react';
import { Intent, Menu, MenuDivider, MenuItem } from '@blueprintjs/core';
import { observer, inject } from 'mobx-react';
import PopupStore from 'stores/PopupStore';
import PopupType from 'enums/PopupType';
import { ITreeElement } from 'stores/TreeStore/BaseTreeStore';
import TreeStore from 'stores/TreeStore';

interface Props {
    popupStore?: PopupStore;
    treeStore?: TreeStore;
    itemId: number;
    node: ITreeElement;
}

@inject('popupStore', 'treeStore')
@observer
export default class ArchiveTreeMenu extends React.Component<Props> {

    private updateClick = () => {
        this.props.treeStore.updateSubTree();
    }

    private restoreClick = () => {
        const { popupStore, itemId } = this.props;
        popupStore.show(itemId, PopupType.RESTORE, 'Восстановить раздел');
    }

    private deleteClick = () => {
        const { popupStore, itemId } = this.props;
        popupStore.show(itemId, PopupType.DELETE, 'Удалить из архива');
    }

    private handleClick = (e: React.MouseEvent<HTMLElement>, cb: () => void) => {
        e.stopPropagation();
        this.props.node.isContextMenuActive = false;
        cb();
    }

    render() {
        return (
            <Menu>
                <MenuItem
                    onClick={(e: React.MouseEvent<HTMLElement>) => this.handleClick(e, this.updateClick)}
                    icon="refresh"
                    text="Обновить"
                />
                <MenuItem
                    onClick={(e: React.MouseEvent<HTMLElement>) => this.handleClick(e, this.restoreClick)}
                    icon="swap-horizontal"
                    text="Восстановить"/>
                <MenuDivider/>
                <MenuItem
                    onClick={(e: React.MouseEvent<HTMLElement>) => this.handleClick(e, this.deleteClick)}
                    icon="delete"
                    text="Удалить"
                    intent={Intent.DANGER}/>
            </Menu>
        );
    }
}
