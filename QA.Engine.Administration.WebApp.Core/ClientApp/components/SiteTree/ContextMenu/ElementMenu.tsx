import * as React from 'react';
import { Intent, Menu, MenuDivider, MenuItem } from '@blueprintjs/core';
import { observer, inject } from 'mobx-react';
import QpIntegrationStore from 'stores/QpIntegrationStore';
import SiteTreeStore from 'stores/TreeStore/SiteTreeStore';
import ArchiveTreeStore from 'stores/TreeStore/ArchiveTreeStore';
import PopupStore from 'stores/PopupStore';
import PopupType from 'enums/PopupType';
import { ITreeElement } from 'stores/TreeStore/BaseTreeStore';
import NavigationStore, { Pages } from 'stores/NavigationStore';
import TreeStore from 'stores/TreeStore';

interface Props {
    qpIntegrationStore?: QpIntegrationStore;
    navigationStore?: NavigationStore;
    popupStore?: PopupStore;
    treeStore?: TreeStore;
    itemId: number;
    node: ITreeElement;
}

@inject('qpIntegrationStore', 'navigationStore', 'popupStore', 'treeStore')
@observer
export default class ElementMenu extends React.Component<Props> {

    private editClick = () => {
        const { qpIntegrationStore, itemId } = this.props;
        qpIntegrationStore.edit(itemId);
    }

    private addClick = () => {
        const { popupStore, itemId } = this.props;
        popupStore.show(itemId, PopupType.ADD, 'Добавить раздел');
    }

    private addVersionClick = () => {
        const { popupStore, itemId } = this.props;
        popupStore.show(itemId, PopupType.ADDVERSION, 'Добавить раздел');
    }

    private historyClick = () => {
        const { qpIntegrationStore, itemId } = this.props;
        qpIntegrationStore.history(itemId);
    }

    private publishClick = () => {
        const { itemId, treeStore } = this.props;
        (treeStore.resolveTreeStore() as SiteTreeStore).publish([itemId]);
    }

    private archiveClick = () => {
        const { popupStore, itemId } = this.props;
        popupStore.show(itemId, PopupType.ARCHIVE, 'Отправить в архив');
    }

    private updateClick = () => {
        const { treeStore, itemId } = this.props;
        (treeStore.resolveTreeStore() as SiteTreeStore).updateSubTree(itemId);
    }

    private updateArchiveClick = () => {
        const { treeStore, itemId } = this.props;
        (treeStore.resolveTreeStore() as ArchiveTreeStore).updateSubTree(itemId);
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

    private handlerExample = () => {
        // example method for menu action. Will be taken from some store in the future.
        console.log('click');
    }

    render() {
        const { navigationStore } = this.props;
        if (navigationStore.currentPage === Pages.ARCHIVE) {
            return (
                <Menu>
                    <MenuItem
                        onClick={(e: React.MouseEvent<HTMLElement>) => this.handleClick(e, this.updateArchiveClick)}
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

        return (
            <Menu>
                <MenuItem
                    onClick={(e: React.MouseEvent<HTMLElement>) => this.handleClick(e, this.updateClick)}
                    icon="refresh"
                    text="Обновить"
                />
                <MenuItem
                    onClick={(e: React.MouseEvent<HTMLElement>) => this.handleClick(e, this.handlerExample)}
                    icon="eye-open"
                    text="Просмотр"
                />
                <MenuItem
                    onClick={(e: React.MouseEvent<HTMLElement>) => this.handleClick(e, this.historyClick)}
                    icon="history"
                    text="История изменений"
                />
                <MenuDivider/>
                <MenuItem
                    onClick={(e: React.MouseEvent<HTMLElement>) => this.handleClick(e, this.publishClick)}
                    icon="confirm"
                    text="Публиковать"
                    intent={Intent.SUCCESS}
                />
                <MenuDivider/>
                <MenuItem
                    onClick={(e: React.MouseEvent<HTMLElement>) => this.handleClick(e, this.addClick)}
                    icon="new-object"
                    text="Добавить подраздел"
                    intent={Intent.PRIMARY}/>
                <MenuItem
                    onClick={(e: React.MouseEvent<HTMLElement>) => this.handleClick(e, this.addVersionClick)}
                    icon="add"
                    text="Добавить версию"
                    intent={Intent.PRIMARY}/>
                <MenuItem
                    onClick={(e: React.MouseEvent<HTMLElement>) => this.handleClick(e, this.editClick)}
                    icon="edit"
                    text="Редактировать"
                    intent={Intent.PRIMARY}/>
                <MenuDivider/>
                <MenuItem
                    onClick={(e: React.MouseEvent<HTMLElement>) => this.handleClick(e, this.archiveClick)}
                    icon="box"
                    text="Архивировать"
                    intent={Intent.DANGER}/>
            </Menu>
        );
    }
}
