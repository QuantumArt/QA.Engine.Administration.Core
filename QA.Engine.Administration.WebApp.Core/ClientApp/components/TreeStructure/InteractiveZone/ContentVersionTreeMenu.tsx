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
export default class ContentVersionTreeMenu extends React.Component<Props> {

    private previewClick = () => {
        const { qpIntegrationStore, itemId } = this.props;
        qpIntegrationStore.preview(itemId);
    }

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
        treeStore.publish([itemId]);
    }

    private archiveClick = () => {
        const { popupStore, itemId, textStore } = this.props;
        popupStore.show(itemId, PopupType.ARCHIVECONTENTVERSION, textStore.texts[Texts.popupArchiveItemTitle]);
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
                    onClick={(e: React.MouseEvent<HTMLElement>) => this.handleClick(e, this.editClick)}
                    icon="edit"
                    text={textStore.texts[Texts.edit]}
                    intent={Intent.PRIMARY}/>
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
