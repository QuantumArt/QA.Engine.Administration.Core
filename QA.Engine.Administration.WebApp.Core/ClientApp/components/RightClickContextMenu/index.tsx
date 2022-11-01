import React from 'react'
import { ContextMenuTarget } from '@blueprintjs/core'
import { inject, observer, Provider } from 'mobx-react'
import QpIntegrationStore from 'stores/QpIntegrationStore'

type Props = {
    content: JSX.Element 
}

type State = {}

@ContextMenuTarget
export default class RightClickContextMenu extends React.Component<Props> {
    public render() {
        // root element must support `onContextMenu`
        return <div>{this.props.children}</div>;
    }
 
    public renderContextMenu() {
        // return a single element, or nothing to use default browser behavior
        return this.props.content;
    }
 
    public onContextMenuClose() {
        // Optional method called once the context menu is closed.
    }
}