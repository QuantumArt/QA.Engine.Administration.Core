import React from 'react'
import {ContextMenuTarget} from '@blueprintjs/core/lib/esnext/components/context-menu/contextMenuTarget.js';

type Props = {
    content: JSX.Element 
}

const RightClickMenu = ContextMenuTarget(class RightClickContextMenu extends React.Component<Props> {
    public render() {
        return <div>{this.props.children}</div>;
    }
 
    public renderContextMenu() {
        return this.props.content;
    }
});

export default RightClickMenu