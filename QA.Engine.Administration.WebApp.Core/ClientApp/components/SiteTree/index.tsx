import * as React from 'react';
import { inject, observer } from 'mobx-react';
import { Card, Spinner, Tree } from '@blueprintjs/core';
import Scrollbars from 'react-custom-scrollbars';
import { NavigationState } from 'stores/NavigationStore';
import OperationState from 'enums/OperationState';
import { ITreeElement } from 'stores/BaseTreeStore';
import { EditArticleState } from 'stores/EditArticleStore';

@observer
class TreeR extends Tree {
}

interface Props {
    navigationStore?: NavigationState;
    editArticleStore?: EditArticleState;
}

interface InternalStyle extends JSX.IntrinsicAttributes, React.ClassAttributes<HTMLDivElement>, React.HTMLAttributes<HTMLDivElement> {
}

interface InternalRestProps extends JSX.IntrinsicAttributes, React.ClassAttributes<HTMLDivElement>, React.HTMLAttributes<HTMLDivElement> {
}

@inject('navigationStore', 'editArticleStore')
@observer
export default class SiteTree extends React.Component<Props> {
    private handleNodeClick = (e: ITreeElement) => {
        const { navigationStore, editArticleStore } = this.props;
        const treeStore = navigationStore.resolveTreeStore();
        treeStore.handleNodeClick(e);
        navigationStore.setDefaultTab(e.isSelected);
        editArticleStore.init(treeStore.selectedNode);
    }

    render() {
        const { navigationStore } = this.props;
        const treeStore = navigationStore.resolveTreeStore();
        const isLoading = treeStore.treeState === OperationState.NONE || treeStore.treeState === OperationState.PENDING;

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
                            contents={treeStore.tree}
                            onNodeCollapse={treeStore.handleNodeCollapse}
                            onNodeExpand={treeStore.handleNodeExpand}
                            onNodeClick={this.handleNodeClick}
                        />
                    </Scrollbars>
                }
            </Card>
        );
    }
}
