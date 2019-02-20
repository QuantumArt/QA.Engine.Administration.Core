import * as React from 'react';
import { inject, observer } from 'mobx-react';
import { Button, Popover, Position } from '@blueprintjs/core';
import { ITreeElement } from 'stores/BaseTreeStore';
import ElementMenu from './ElementMenu';
import { NavigationState } from 'stores/NavigationStore';

interface Props {
    navigationStore?: NavigationState;
    node: ITreeElement;
}

@inject('navigationStore')
@observer
export default class ContextMenu extends React.Component<Props> {
    private handleClick = (e: React.MouseEvent<HTMLElement>) => {
        e.stopPropagation();
        const { navigationStore, node } = this.props;
        const treeStore = navigationStore.resolveTreeStore();
        treeStore.handleContextMenu(node);
    }

    render() {
        const { node } = this.props;
        const elementMenu = <ElementMenu itemId={+node.id} node={node} />;
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
