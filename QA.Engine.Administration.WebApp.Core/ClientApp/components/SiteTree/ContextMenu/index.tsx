import * as React from 'react';
import { inject, observer } from 'mobx-react';
import { Button, Popover, Position } from '@blueprintjs/core';
import { ITreeElement } from 'stores/TreeStore/BaseTreeStore';
import ContentVersionTreeMenu from './ContentVersionTreeMenu';
import TreeStore from 'stores/TreeStore';
import ContextMenuType from 'enums/ContextMenuType';
import SiteTreeMenu from './SiteTreeMenu';
import ArchiveTreeMenu from './ArchiveTreeMenu';
import WidgetTreeMenu from './WidgetTreeMenu';

interface Props {
    treeStore?: TreeStore;
    node: ITreeElement;
}

@inject('treeStore')
@observer
export default class ContextMenu extends React.Component<Props> {
    private handleClick = (e: React.MouseEvent<HTMLElement>) => {
        e.stopPropagation();
        const { treeStore, node } = this.props;
        treeStore.resolveTreeStore().handleContextMenu(node);
    }

    render() {
        const { node } = this.props;
        let elementMenu: JSX.Element;
        switch (node.contextMenuType) {
        case ContextMenuType.SITEMAP:
            elementMenu = <SiteTreeMenu itemId={+node.id} node={node} />;
            break;
        case ContextMenuType.ARCHIVE:
            elementMenu = <ArchiveTreeMenu itemId={+node.id} node={node} />;
            break;
        case ContextMenuType.CONTENTVERSION:
            elementMenu = <ContentVersionTreeMenu itemId={+node.id} node={node} />;
            break;
        case ContextMenuType.WIDGET:
            elementMenu = <WidgetTreeMenu itemId={+node.id} node={node} />;
        default:
            break;
        }

        return node.isSelected ?
            <Popover
                content={elementMenu}
                position={Position.RIGHT}
                autoFocus={false}
                isOpen={node.isContextMenuActive}
                boundary="viewport"
            >
                <Button
                    icon="cog"
                    minimal
                    onClick={this.handleClick}
                    className="context-button"
                />
            </Popover>
            : null;
    }
}
