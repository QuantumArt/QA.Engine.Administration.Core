import * as React from 'react';
import { Intent, Menu, MenuDivider, MenuItem } from '@blueprintjs/core';
import { observer, inject } from 'mobx-react';
import QpIntegrationStore from 'stores/QpIntegrationStore';
import PopupStore from 'stores/PopupStore';
import PopupType from 'enums/PopupType';
import { ITreeElement } from 'stores/TreeStore/BaseTreeStore';
import TreeStore from 'stores/TreeStore';
import TextStore from 'stores/TextStore';
import Texts from 'constants/Texts';

interface Props {
    qpIntegrationStore?: QpIntegrationStore;
    popupStore?: PopupStore;
    treeStore?: TreeStore;
    textStore?: TextStore;
    itemId: number;
    node: ITreeElement;
}

@inject('qpIntegrationStore', 'popupStore', 'treeStore', 'textStore')
@observer
export default class SiteTreeMenu extends React.Component<Props> {

    private previewClick = async () => {
        const { qpIntegrationStore, itemId, treeStore } = this.props;
        const tree = treeStore.getSiteTreeStore();
        const root = await tree.getRootElement();
        qpIntegrationStore.preview(itemId, root.alias.trim());
    }

    private editClick = () => {
        const { qpIntegrationStore, itemId } = this.props;
        qpIntegrationStore.edit(itemId);
    }

    private addClick = () => {
        const { popupStore, itemId, textStore } = this.props;
        popupStore.show(itemId, PopupType.ADD, textStore.texts[Texts.popupAddItemTitle]);
    }

    private addVersionClick = () => {
        const { popupStore, itemId, textStore } = this.props;
        popupStore.show(itemId, PopupType.ADDVERSION, textStore.texts[Texts.popupAddVersionItemTitle]);
    }

    private historyClick = () => {
        const { qpIntegrationStore, itemId } = this.props;
        qpIntegrationStore.history(itemId);
    }

    private publishClick = () => {
        const { itemId, treeStore } = this.props;
        treeStore.publish([itemId]);
    }

    private archiveClick = () => {
        const { popupStore, itemId, textStore } = this.props;
        popupStore.show(itemId, PopupType.ARCHIVE, textStore.texts[Texts.popupArchiveItemTitle]);
    }

    private updateClick = () => {
        this.props.treeStore.updateSubTree();
    }

    private reorderClick = () => {
        const { popupStore, itemId, textStore } = this.props;
        popupStore.show(itemId, PopupType.REORDER, textStore.texts[Texts.popupReorderTitle]);
    }

    private moveClick = () => {
        const { popupStore, itemId, textStore, treeStore } = this.props;
        const tree = treeStore.getSiteTreeStore();
        treeStore.getMoveTreeStore().init(tree.selectedNode, tree.origTree);
        popupStore.show(itemId, PopupType.MOVE, textStore.texts[Texts.popupMoveTitle]);
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
                    onClick={(e: React.MouseEvent<HTMLElement>) => this.handleClick(e, this.previewClick)}
                    icon="eye-open"
                    text={textStore.texts[Texts.view]}
                />
                <MenuItem
                    onClick={(e: React.MouseEvent<HTMLElement>) => this.handleClick(e, this.historyClick)}
                    icon="history"
                    text={textStore.texts[Texts.history]}
                />
                <MenuDivider/>
                <MenuItem
                    onClick={(e: React.MouseEvent<HTMLElement>) => this.handleClick(e, this.publishClick)}
                    icon="confirm"
                    text={textStore.texts[Texts.publish]}
                    intent={Intent.SUCCESS}
                />
                <MenuDivider/>
                <MenuItem
                    onClick={(e: React.MouseEvent<HTMLElement>) => this.handleClick(e, this.addClick)}
                    icon="new-object"
                    text={textStore.texts[Texts.addItem]}
                    intent={Intent.PRIMARY}/>
                <MenuItem
                    onClick={(e: React.MouseEvent<HTMLElement>) => this.handleClick(e, this.addVersionClick)}
                    icon="add"
                    text={textStore.texts[Texts.addVersionItem]}
                    intent={Intent.PRIMARY}/>
                <MenuItem
                    onClick={(e: React.MouseEvent<HTMLElement>) => this.handleClick(e, this.editClick)}
                    icon="edit"
                    text={textStore.texts[Texts.edit]}
                    intent={Intent.PRIMARY}/>
                <MenuItem
                    onClick={(e: React.MouseEvent<HTMLElement>) => this.handleClick(e, this.reorderClick)}
                    icon="sort"
                    text={textStore.texts[Texts.reorder]}
                    intent={Intent.PRIMARY}
                />
                <MenuItem
                    onClick={(e: React.MouseEvent<HTMLElement>) => this.handleClick(e, this.moveClick)}
                    icon="move"
                    text={textStore.texts[Texts.move]}
                    intent={Intent.PRIMARY}
                />
                <MenuDivider/>
                <MenuItem
                    onClick={(e: React.MouseEvent<HTMLElement>) => this.handleClick(e, this.archiveClick)}
                    icon="box"
                    text={textStore.texts[Texts.archive]}
                    intent={Intent.DANGER}/>
            </Menu>
        );
    }
}
