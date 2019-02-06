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
    private handleClick = () => {
        const { siteTreeStore, node } = this.props;
        siteTreeStore.handleContextMenu(node);
    }

    render() {
        return (
            <Popover
                content={<ElementMenu />}
                position={Position.RIGHT}
                autoFocus={false}
            >
                <Button
                    icon="cog"
                    minimal
                    onClick={this.handleClick}
                />
            </Popover>
        );
    }
}
