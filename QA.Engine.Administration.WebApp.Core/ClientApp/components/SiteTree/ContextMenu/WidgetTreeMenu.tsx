import * as React from 'react';
import { Intent, Menu, MenuDivider, MenuItem } from '@blueprintjs/core';
import { observer, inject } from 'mobx-react';
import QpIntegrationStore from 'stores/QpIntegrationStore';
import PopupStore from 'stores/PopupStore';
import PopupType from 'enums/PopupType';
import { ITreeElement } from 'stores/TreeStore/BaseTreeStore';
import TreeStore from 'stores/TreeStore';

interface Props {
    qpIntegrationStore?: QpIntegrationStore;
    popupStore?: PopupStore;
    treeStore?: TreeStore;
    itemId: number;
    node: ITreeElement;
}

@inject('qpIntegrationStore', 'popupStore', 'treeStore')
@observer
export default class WidgetTreeMenu extends React.Component<Props> {

    private editClick = () => {
        const { qpIntegrationStore, itemId } = this.props;
        qpIntegrationStore.edit(itemId);
    }

    private historyClick = () => {
        const { qpIntegrationStore, itemId } = this.props;
        qpIntegrationStore.history(itemId);
    }

    private publishClick = () => {
        const { treeStore, itemId } = this.props;
        treeStore.getWidgetStore().publish([itemId]);
    }

    private archiveClick = () => {
        const { popupStore, itemId } = this.props;
        popupStore.show(itemId, PopupType.ARCHIVE, 'Отправить в архив');
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
        return (
            <Menu>
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
