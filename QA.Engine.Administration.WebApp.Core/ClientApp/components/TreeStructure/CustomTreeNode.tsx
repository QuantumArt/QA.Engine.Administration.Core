import * as React from 'react';
import { TreeNode, ITreeNodeProps } from '@blueprintjs/core';

export class CustomTreeNode<T = {}> extends React.Component<ITreeNodeProps<T>, {}> {

    public static ofType<T>() {
        return CustomTreeNode as new (props: ITreeNodeProps<T>) => CustomTreeNode<T>;
    }

    shouldComponentUpdate(nextProps: ITreeNodeProps<T>): boolean {
        if (this.props.isExpanded === nextProps.isExpanded && this.props.isSelected === nextProps.isSelected) {
            // console.debug(`%cshouldComponentUpdate false ${this.props.label}`, 'color: red;');
            return false;
        }
        // console.debug(`%cshouldComponentUpdate true ${this.props.label}`, 'color: green;');
        return true;
    }

    render() {
        // tslint:disable-next-line:variable-name
        const TypedTreeNode = TreeNode.ofType<T>();
        return <TypedTreeNode {...this.props} />;
    }
}
