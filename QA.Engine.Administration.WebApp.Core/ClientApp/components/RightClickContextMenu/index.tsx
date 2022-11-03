import React, { ComponentClass } from "react";
import { ContextMenuTarget } from "@blueprintjs/core/lib/esnext/components/context-menu/contextMenuTarget.js";
import { ContextMenu } from "@blueprintjs/core";

type Props = {
    content: JSX.Element;
};

class RightClickMenu extends React.PureComponent<Props> {
    render() {
        return (
            <div onContextMenu={this.showContextMenu}>
                {this.props.children}
            </div>
        );
    }

    private showContextMenu = (e: React.MouseEvent<HTMLDivElement>) => {
        e.preventDefault();
        ContextMenu.show(this.props.content, {
            left: e.clientX,
            top: e.clientY,
        });
    };
}

export default RightClickMenu;
