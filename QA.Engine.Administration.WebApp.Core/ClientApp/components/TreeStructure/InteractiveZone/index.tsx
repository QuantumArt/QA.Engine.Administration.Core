import * as React from 'react';
import { inject, observer } from 'mobx-react';
import cn from 'classnames'; // tslint:disable-line
import { Button, Popover, Position } from '@blueprintjs/core';
import { ITreeElement } from 'stores/TreeStore/BaseTreeStore';
import ContentVersionTreeMenu from './ContentVersionTreeMenu';
import TreeStore from 'stores/TreeStore';
import NavigationStore, { TabTypes } from 'stores/NavigationStore';
import ContextMenuType from 'enums/ContextMenuType';
import TreeStoreType from 'enums/TreeStoreType';
import SiteTreeMenu from './SiteTreeMenu';
import ArchiveTreeMenu from './ArchiveTreeMenu';
import WidgetTreeMenu from './WidgetTreeMenu';
import SiteTreeStore from 'stores/TreeStore/SiteTreeStore';

interface Props {
    treeStore?: TreeStore;
    navigationStore?: NavigationStore;
    node: ITreeElement;
    type: TreeStoreType;
}

interface State {
    isHovered: boolean;
}

@inject('treeStore', 'navigationStore')
@observer
export default class InteractiveZone extends React.Component<Props, State> {
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
                treeStore.getSiteTreeStore().handleContextMenu(node);
            }
            if (node.contextMenuType === ContextMenuType.ARCHIVE) {
                treeStore.getArchiveTreeStore().handleContextMenu(node);
            }
            if (node.contextMenuType === ContextMenuType.CONTENTVERSION) {
                treeStore.getContentVersionTreeStore().handleContextMenu(node);
            }
            if (node.contextMenuType === ContextMenuType.WIDGET) {
                treeStore.getWidgetTreeStore().handleContextMenu(node);
            }
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

    private handleExpandToNodeClick = (node: ITreeElement) => (e: React.MouseEvent<HTMLElement>) => {
        const { navigationStore, treeStore, type } = this.props;
        e.stopPropagation();
        const tree = treeStore.resolveTree(type);
        tree.setSelectedNode(node);
        if (navigationStore.currentTab === TabTypes.NONE) {
            navigationStore.changeTab(TabTypes.COMMON);
        }
        tree.expandToNode(node);
        tree.resetSearch();
    }

    componentWillUnmount() {
        window.removeEventListener('click', this.stateHandler);
        window.removeEventListener('mouseenter', this.toggleHover);
        window.removeEventListener('mouseleave', this.toggleHover);
        window.removeEventListener('mouseup', this.toggleHover);
    }

    render() {
        const { node, treeStore, type } = this.props;
        const tree = treeStore.resolveTree(type);
        const showExpandBtn = tree.searchActive && type !== TreeStoreType.ARCHIVE;
        let isMoveTreeMode = false;
        let elementMenu: JSX.Element;
        switch (node.contextMenuType) {
            case ContextMenuType.SITEMAP:
                elementMenu = <SiteTreeMenu itemId={+node.id} node={node} />;
                isMoveTreeMode = tree instanceof SiteTreeStore ? tree.moveTreeMode : false;
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

        return (
            <React.Fragment>
                {showExpandBtn &&
                    <Button
                        icon="diagram-tree"
                        minimal
                        onClick={this.handleExpandToNodeClick(node)}
                        className={cn('context-button expand-to-node', {
                            'expand-to-node--active': node.isSelected && !isMoveTreeMode,
                        })}
                    />
                }
                {(node.isSelected && !isMoveTreeMode) &&
                    <Popover
                        className="context-popover"
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
                    </Popover>
                }
            </React.Fragment>);
    }
}
