import * as React from 'react';
import { inject, observer } from 'mobx-react';
import { Button, Popover, Position } from '@blueprintjs/core';
import SiteTreeStore, { ITreeElement } from 'stores/SiteTreeStore';
import ElementMenu from './ElementMenu';

interface Props {
    siteTreeStore?: SiteTreeStore;
    node: ITreeElement;
}

@inject('siteTreeStore')
@observer
export default class ContextMenu extends React.Component<Props> {
    private handleClick = (e: React.MouseEvent<HTMLElement>) => {
        e.stopPropagation();
        const { siteTreeStore, node } = this.props;
        siteTreeStore.handleContextMenu(node);
    }

    render() {
        const { node } = this.props;
        return node.isSelected ?
            <Popover
                content={<ElementMenu />}
                position={Position.RIGHT}
                autoFocus={false}
                isOpen={node.isContextMenuActive}
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
