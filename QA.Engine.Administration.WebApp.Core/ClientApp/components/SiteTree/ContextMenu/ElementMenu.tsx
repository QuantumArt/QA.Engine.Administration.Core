import * as React from 'react';
import { Intent, Menu, MenuDivider, MenuItem } from '@blueprintjs/core';
import { observer, inject } from 'mobx-react';
import { QpIntegrationState } from 'stores/QpIntegrationStore';
import { SiteTreeState } from 'stores/SiteTreeStore';
import { ArchiveState } from 'stores/ArchiveStore';

interface Props {
    qpIntegrationStore?: QpIntegrationState;
    siteTreeStore?: SiteTreeState;
    archiveStore?: ArchiveState;
    itemId: number;
}

@inject('qpIntegrationStore', 'siteTreeStore', 'archiveStore')
@observer
export default class ElementMenu extends React.Component<Props> {

    private editClick = () => {
        const { qpIntegrationStore, itemId } = this.props;
        qpIntegrationStore.edit(itemId);
    }

    private addClick = () => {
        const { qpIntegrationStore, siteTreeStore, itemId } = this.props;
        const node = siteTreeStore.getNodeById(itemId);
        qpIntegrationStore.add(node, null, `test_page_${Date.now()}`, `test page ${Date.now()}`, 741035, 0);
    }

    private addVersionClick = () => {
        const { qpIntegrationStore, siteTreeStore, itemId } = this.props;
        const node = siteTreeStore.getNodeById(itemId);
        qpIntegrationStore.add(node, 'Structural', node.alias, `${node.title} (struct v.)`, 741035, 0);
    }

    private historyClick = () => {
        const { qpIntegrationStore, itemId } = this.props;
        qpIntegrationStore.history(itemId);
    }

    private publishClick = () => {
        const { siteTreeStore, itemId } = this.props;
        siteTreeStore.publish([itemId]);
    }

    private removeClick = () => {
        const { siteTreeStore, itemId } = this.props;
        const model: RemoveModel = {
            itemId,
            isDeleteAllVersions: true,
            isDeleteContentVersions: true,
            contentVersionId: null,
        };
        siteTreeStore.remove(model);
    }

    private updateClick = () => {
        const { siteTreeStore, itemId } = this.props;
        siteTreeStore.updateSubTree(itemId);
    }

    private restoreClick = () => {
        const { archiveStore, itemId } = this.props;
        const model: RestoreModel = {
            itemId,
            isRestoreAllVersions: true,
            isRestoreWidgets: true,
            isRestoreContentVersions: true,
            isRestoreChildren: true,
        };
        archiveStore.restore(model);
    }

    private deleteClick = () => {
        const { archiveStore, itemId } = this.props;
        const model: DeleteModel = {
            itemId,
            isDeleteAllVersions: true,
        };
        archiveStore.delete(model);
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
        const { archiveStore, itemId } = this.props;
        let isArchive = false;
        if (archiveStore.getNodeById(itemId) != null) {
            isArchive = true;
        }
        if (isArchive === true) {
            return (
                <Menu>
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
                    onClick={(e: React.MouseEvent<HTMLElement>) => this.handleClick(e, this.removeClick)}
                    icon="box"
                    text="Архивировать"
                    intent={Intent.DANGER}/>
            </Menu>
        );
    }
}
