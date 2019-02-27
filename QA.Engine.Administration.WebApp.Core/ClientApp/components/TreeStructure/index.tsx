import * as React from 'react';
import { inject, observer } from 'mobx-react';
import { Card, Spinner } from '@blueprintjs/core';
import Scrollbars from 'react-custom-scrollbars'; // tslint:disable-line
import cn from 'classnames'; // tslint:disable-line
import NavigationStore  from 'stores/NavigationStore';
import OperationState from 'enums/OperationState';
import { ITreeElement } from 'stores/TreeStore/BaseTreeStore';
import EditArticleStore from 'stores/EditArticleStore';
import TreeStore from 'stores/TreeStore';
import { CustomTree } from 'components/TreeStructure/CustomTree';
import SiteTreeStore from 'stores/TreeStore/SiteTreeStore';
import ArchiveTreeStore from 'stores/TreeStore/ArchiveTreeStore';
import WidgetTreeStore from 'stores/TreeStore/WidgetTreeStore';
import ContentVersionTreeStore from 'stores/TreeStore/ContentVersionTreeStore';

interface Props {
    type: 'site' | 'archive' | 'widgets' | 'versions';
    treeStore: TreeStore;
    navigationStore?: NavigationStore;
    editArticleStore?: EditArticleStore;
    sbHeightMin?: number;
    sbHeightMax?: number;
    sbThumbSize?: number;
    spinnerSize?: number;
    className?: string;
}

type DefaultProps = 'sbHeightMin' | 'sbHeightMax' | 'sbThumbSize' | 'spinnerSize';

interface InternalStyle extends JSX.IntrinsicAttributes, React.ClassAttributes<HTMLDivElement>, React.HTMLAttributes<HTMLDivElement> {
}

interface InternalRestProps extends JSX.IntrinsicAttributes, React.ClassAttributes<HTMLDivElement>, React.HTMLAttributes<HTMLDivElement> {
}

@observer
class TreeR extends CustomTree {
}

@inject('navigationStore', 'editArticleStore')
@observer
export default class SiteTree extends React.Component<Props> {

    static defaultProps: Pick<Props, DefaultProps> = {
        sbHeightMin: 30,
        sbHeightMax: 855,
        spinnerSize: 30,
    };

    tree: SiteTreeStore | ArchiveTreeStore | WidgetTreeStore | ContentVersionTreeStore;

    private resolveTree = () => {
        const { type, treeStore } = this.props;
        if (type === 'site' || type === 'archive') {
            this.tree = treeStore.resolveTreeStore();
        }
        if (type === 'widgets') {
            this.tree = treeStore.getWidgetStore();
        }
        if (type === 'versions') {
            this.tree = treeStore.getContentVersionsStore();
        }
    }

    private handleMajorTreeNode = (e: ITreeElement) => {
        const { navigationStore, editArticleStore, treeStore } = this.props;
        this.tree.handleNodeClick(e);
        navigationStore.setDefaultTab(e.isSelected);
        [
            editArticleStore,
            treeStore.getContentVersionsStore(),
            treeStore.getWidgetStore(),
        ].forEach((x) => {
            if (this.tree instanceof SiteTreeStore || this.tree instanceof ArchiveTreeStore) {
                x.init(this.tree.selectedNode);
            }
        });
    }

    private handleMinorTreeNode = (e: ITreeElement) => {
        this.tree.handleNodeClick(e);
        if (!e.isSelected) {
            this.tree.selectedNode = null;
        }
    }

    private handleNodeClick = (e: ITreeElement) => {
        const { type } = this.props;
        if (type === 'site' || type === 'archive') {
            this.handleMajorTreeNode(e);
        } else {
            this.handleMinorTreeNode(e);
        }
    }

    render() {
        this.resolveTree();
        const isLoading = this.tree.treeState === OperationState.NONE || this.tree.treeState === OperationState.PENDING;
        return (
            <Card className={cn('tree-pane', this.props.className)}>
                {isLoading ?
                    <Spinner size={this.props.spinnerSize}/> :
                    <Scrollbars
                        hideTracksWhenNotNeeded
                        autoHeight
                        autoHide
                        autoHeightMin={this.props.sbHeightMin}
                        autoHeightMax={this.props.sbHeightMax}
                        thumbMinSize={this.props.sbThumbSize}
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
                            className="tree"
                            contents={this.tree.tree}
                            onNodeCollapse={this.tree.handleNodeCollapse}
                            onNodeExpand={this.tree.handleNodeExpand}
                            onNodeClick={this.handleNodeClick}
                        />
                    </Scrollbars>
                }
            </Card>
        );
    }
}
