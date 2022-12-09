import React from "react";
import { Position, Tooltip } from "@blueprintjs/core";

function isTextOverflow(element: HTMLDivElement) {
    return element.clientWidth < element.scrollWidth;
}

interface Props {
    title: string;
    children: JSX.Element;
}

interface State {
    overflow: boolean;
    width: number;
    height: number;
}

export default class OverflowTooltip extends React.Component<Props, State> {
    state = {
        overflow: false,
        width: 0,
        height: 0,
    };

    ref = React.createRef<HTMLDivElement>();

    updateDimensions = () => {
        this.setState({ width: window.innerWidth, height: window.innerHeight });
      };

    componentDidMount() {
        window.addEventListener('resize', this.updateDimensions);
        this.checkOverflow();
    }

    componentWillUnmount() {
        window.removeEventListener('resize', this.updateDimensions);
      }

    componentDidUpdate() {
        this.checkOverflow();
    }

    checkOverflow() {

        const overflow = isTextOverflow(this.ref.current);
        if (overflow !== this.state.overflow) {
            this.setState({ overflow: overflow });
        }
    }

    render() {
        return (
            <div ref={this.ref}>
                {this.state.overflow ? (
                    <Tooltip
                        content={`${this.props.title}`}
                        boundary="viewport"
                        position={Position.BOTTOM}
                        modifiers={{
                            arrow: { enabled: false },
                        }}
                    >
                        {this.props.children}
                    </Tooltip>
                ) : (
                    this.props.children
                )}
            </div>
        );
    }
}
