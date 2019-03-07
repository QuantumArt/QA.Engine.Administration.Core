import * as React from 'react';
import { TreeNode, ITreeNodeProps } from '@blueprintjs/core';

export class CustomTreeNode<T = {}> extends React.Component<ITreeNodeProps<T>, {}> {

    public static ofType<T>() {
        return CustomTreeNode as new (props: ITreeNodeProps<T>) => CustomTreeNode<T>;
    }

    shouldComponentUpdate(nextProps: ITreeNodeProps<T>): boolean {
        return !(this.props.isExpanded === nextProps.isExpanded && this.props.isSelected === nextProps.isSelected);
    }

    render() {
        // tslint:disable-next-line:variable-name
        const TypedTreeNode = TreeNode.ofType<T>();
        return <TypedTreeNode {...this.props} />;
    }
}
