import * as React from 'react';
import { inject, observer } from 'mobx-react';
import { Card, Spinner, Tree } from '@blueprintjs/core';
import Scrollbars from 'react-custom-scrollbars'; // tslint:disable-line
import NavigationStore from 'stores/NavigationStore';
import OperationState from 'enums/OperationState';
import { ITreeElement } from 'stores/TreeStore/BaseTreeStore';
import EditArticleState from 'stores/EditArticleStore';
import TreeStore from 'stores/TreeStore';

@observer
class TreeR extends Tree {
}

interface Props {
    navigationStore?: NavigationStore;
    editArticleStore?: EditArticleState;
    treeStore?: TreeStore;
}

interface InternalStyle extends JSX.IntrinsicAttributes, React.ClassAttributes<HTMLDivElement>, React.HTMLAttributes<HTMLDivElement> {
}

interface InternalRestProps extends JSX.IntrinsicAttributes, React.ClassAttributes<HTMLDivElement>, React.HTMLAttributes<HTMLDivElement> {
}

@inject('navigationStore', 'editArticleStore', 'treeStore')
@observer
export default class SiteTree extends React.Component<Props> {
    private handleNodeClick = (e: ITreeElement) => {
        const { navigationStore, editArticleStore, treeStore } = this.props;
        const tree = treeStore.resolveTreeStore();
        tree.handleNodeClick(e);
        navigationStore.setDefaultTab(e.isSelected);
        editArticleStore.init(tree.selectedNode);
    }

    render() {
        const { treeStore } = this.props;
        const tree = treeStore.resolveTreeStore();
        const isLoading = tree.treeState === OperationState.NONE || tree.treeState === OperationState.PENDING;

        return (
            <Card className="tree-pane">
                {isLoading ?
                    <Spinner size={30}/> :
                    <Scrollbars
                        autoHeight
                        autoHide
                        autoHeightMin={30}
                        autoHeightMax={855}
                        thumbMinSize={100}
                        renderTrackVertical={(style: InternalStyle, ...props: InternalRestProps[]) => (
                            <div
                                className="track-vertical"
                                {...props}
                            />
                        )}
                        renderThumbVertical={(style: InternalStyle, ...props: InternalRestProps[]) => (
                            <div
                                className="thumb-vertical"
                                {...props}
                            />
                        )}
                    >
                        <TreeR
                            className="site-tree"
                            contents={tree.tree}
                            onNodeCollapse={tree.handleNodeCollapse}
                            onNodeExpand={tree.handleNodeExpand}
                            onNodeClick={this.handleNodeClick}
                        />
                    </Scrollbars>
                }
            </Card>
        );
    }
}
