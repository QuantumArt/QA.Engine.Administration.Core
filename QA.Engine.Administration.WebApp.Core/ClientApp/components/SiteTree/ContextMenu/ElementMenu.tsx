import * as React from 'react';
import { Intent, Menu, MenuDivider, MenuItem } from '@blueprintjs/core';
import { observer, inject } from 'mobx-react';
import { QpIntegrationState } from 'stores/QpIntegrationStore';
import { SiteTreeState } from 'stores/SiteTreeStore';

interface Props {
    qpIntegrationStore?: QpIntegrationState;
    siteTreeStore?: SiteTreeState;
    itemId: number;
}

@inject('qpIntegrationStore', 'siteTreeStore')
@observer
export default class ElementMenu extends React.Component<Props> {

    private editClick = () => {
        const { qpIntegrationStore, itemId } = this.props;
        qpIntegrationStore.edit(itemId);
    }

    private addClick = () => {
        const { qpIntegrationStore, siteTreeStore, itemId } = this.props;
        const node = siteTreeStore.getNodeById(itemId);
        qpIntegrationStore.add(node, null);
    }

    private addVersionClick = () => {
        const { qpIntegrationStore, siteTreeStore, itemId } = this.props;
        const node = siteTreeStore.getNodeById(itemId);
        qpIntegrationStore.add(node, 'Structural');
    }

    private historyClick = () => {
        const { qpIntegrationStore, itemId } = this.props;
        qpIntegrationStore.history(itemId);
    }

    private handleClick = (e: React.MouseEvent<HTMLElement>, cb: () => void) => {
        e.stopPropagation();
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
                    onClick={(e: React.MouseEvent<HTMLElement>) => this.handleClick(e, this.handlerExample)}
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
                    onClick={(e: React.MouseEvent<HTMLElement>) => this.handleClick(e, this.handlerExample)}
                    icon="box"
                    text="Архивировать"
                    intent={Intent.DANGER}/>
            </Menu>
        );
    }
}
