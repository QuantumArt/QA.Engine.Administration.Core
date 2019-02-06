import * as React from 'react';
import { Intent, Menu, MenuDivider, MenuItem } from '@blueprintjs/core';

interface Props {

}

export default class ElementMenu extends React.Component<Props> {
    render() {
        return (
            <Menu>
                <MenuDivider title="View"/>
                <MenuItem text="Primary" intent={Intent.PRIMARY}/>
                <MenuItem text="Success" intent={Intent.SUCCESS}/>
                <MenuDivider title="Edit"/>
                <MenuItem text="Default"/>
                <MenuItem text="Danger" intent={Intent.DANGER}/>
            </Menu>
        )
    }
}
