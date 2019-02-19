import * as React from 'react';
import { Intent, Menu, MenuDivider, MenuItem } from '@blueprintjs/core';
import { observer, inject } from 'mobx-react';
import { QpIntegrationState } from 'stores/QpIntegrationStore';
import { SiteTreeState } from 'stores/SiteTreeStore';
import { ArchiveState } from 'stores/ArchiveStore';
import { PopupState } from 'stores/PopupStore';
import PopupType from 'enums/PopupType';
import { ITreeElement } from 'stores/BaseTreeStore';

interface Props {
    qpIntegrationStore?: QpIntegrationState;
    siteTreeStore?: SiteTreeState;
    archiveStore?: ArchiveState;
    popupStore?: PopupState;
    itemId: number;
    node: ITreeElement;
}

@inject('qpIntegrationStore', 'siteTreeStore', 'archiveStore', 'popupStore')
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
        const { siteTreeStore, itemId } = this.props;
        siteTreeStore.publish([itemId]);
    }

    private archiveClick = () => {
        const { popupStore, itemId } = this.props;
        popupStore.show(itemId, PopupType.ARCHIVE, 'Отправить в архив');
    }

    private updateClick = () => {
        const { siteTreeStore, itemId } = this.props;
        siteTreeStore.updateSubTree(itemId);
    }

    private updateArchiveClick = () => {
        const { archiveStore, itemId } = this.props;
        archiveStore.updateSubTree(itemId);
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
        const { node } = this.props;
        if (node.isArchive === true) {
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
