import * as React from 'react';
import { inject, observer } from 'mobx-react';
import { Button, Popover, Position, Icon } from '@blueprintjs/core';
import { ITreeElement } from 'stores/TreeStore/BaseTreeStore';
import ContentVersionTreeMenu from './ContentVersionTreeMenu';
import TreeStore from 'stores/TreeStore';
import ContextMenuType from 'enums/ContextMenuType';
import SiteTreeMenu from './SiteTreeMenu';
import ArchiveTreeMenu from './ArchiveTreeMenu';
import WidgetTreeMenu from './WidgetTreeMenu';
import TreeStoreType from 'enums/TreeStoreType';

interface Props {
    treeStore?: TreeStore;
    node: ITreeElement;
}

interface State {
    isHovered: boolean;
}

@inject('treeStore')
@observer
export default class ContextMenu extends React.Component<Props, State> {
    constructor(props: Props) {
        super(props);
        this.state = {
            isHovered: false,
        };
    }

    ref: HTMLDivElement;
    refFunc = (el: HTMLDivElement) => {
        this.ref = el;
        this.bindHover(el);
    }

    private stateHandler = () => {
        const { isHovered } = this.state;
        const { treeStore, node } = this.props;
        if (node.isContextMenuActive) {
            window.removeEventListener('click', this.stateHandler);
        }
        if (!isHovered) {
            if (node.contextMenuType === ContextMenuType.SITEMAP) {
                treeStore.getTreeStore(TreeStoreType.SITE).handleContextMenu(node);
            }
            if (node.contextMenuType === ContextMenuType.ARCHIVE) {
                treeStore.getTreeStore(TreeStoreType.ARCHIVE).handleContextMenu(node);
            }
            if (node.contextMenuType === ContextMenuType.CONTENTVERSION) {
                treeStore.getTreeStore(TreeStoreType.CONTENTVERSION).handleContextMenu(node);
            }
            if (node.contextMenuType === ContextMenuType.WIDGET) {
                treeStore.getTreeStore(TreeStoreType.WIDGET).handleContextMenu(node);
            }
            // treeStore.resolveTreeStore().handleContextMenu(node);
        }
    }

    private handleClick = (e: React.MouseEvent<HTMLElement>) => {
        e.stopPropagation();
        window.addEventListener('click', this.stateHandler, { once: true });
        this.stateHandler();
    }

    private toggleHover = (e: MouseEvent) => {
        if (e.type === 'mouseenter') {
            this.setState({ isHovered: true  });
        } else if (e.type === 'mouseleave') {
            this.setState({ isHovered: false  });
        } else if (e.type === 'mouseup') {
            this.setState({ isHovered: false  });
            window.removeEventListener('click', this.stateHandler);
        }
    }

    private bindHover = (el: HTMLDivElement) => {
        if (el !== null) {
            el.addEventListener('mouseenter', this.toggleHover);
            el.addEventListener('mouseleave', this.toggleHover);
            el.addEventListener('mouseup', this.toggleHover);
        }
    }

    componentWillUnmount() {
        window.removeEventListener('click', this.stateHandler);
        window.removeEventListener('mouseenter', this.toggleHover);
        window.removeEventListener('mouseleave', this.toggleHover);
        window.removeEventListener('mouseup', this.toggleHover);
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
                break;
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
                popoverRef={this.refFunc}
            >
                <Button
                    icon="cog"
                    minimal
                    onClick={this.handleClick}
                    className="context-button"
                />
            </Popover> :
            <React.Fragment>
                <Icon className="context-icon" icon={node.isVisible ? 'eye-open' : 'eye-off'} />
            </React.Fragment>;
    }
}
