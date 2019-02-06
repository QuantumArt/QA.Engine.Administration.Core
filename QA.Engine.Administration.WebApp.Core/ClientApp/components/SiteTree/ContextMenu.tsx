import * as React from 'react';
import { inject, observer } from 'mobx-react';
import { Popover, Menu, MenuItem, Button } from '@blueprintjs/core';
import SiteTreeStore from 'stores/SiteTreeStore';
import { ISiteTreeModel } from 'services/siteTreeService';

interface Props {
    isOpen: boolean;
    siteTreeStore?: SiteTreeStore;
    node: ISiteTreeModel;
}

@inject('siteTreeStore')
@observer
export default class ContextMenu extends React.Component<Props> {
    private handleClick = () => {
        const { siteTreeStore, node } = this.props;
        siteTreeStore.handleContextMenu(node);
    }

    render() {
        const { siteTreeStore } = this.props;
        return (
            <Popover
                isOpen={this.props.isOpen}
                content={
                    <Menu>
                        <MenuItem text="Submenu">
                            <MenuItem text="Child"/>
                        </MenuItem>
                    </Menu>
                }
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
