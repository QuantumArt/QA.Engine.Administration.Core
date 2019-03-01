import * as React from 'react';
import { inject, observer } from 'mobx-react';
import { Card, Spinner } from '@blueprintjs/core';
import Scrollbars from 'react-custom-scrollbars'; // tslint:disable-line
import cn from 'classnames'; // tslint:disable-line
import NavigationStore, { Pages }  from 'stores/NavigationStore';
import OperationState from 'enums/OperationState';
import { ITreeElement } from 'stores/TreeStore/BaseTreeStore';
import EditArticleStore from 'stores/EditArticleStore';
import TreeStore, { TreeType } from 'stores/TreeStore';
import { CustomTree } from 'components/TreeStructure/CustomTree';
import SiteTreeStore from 'stores/TreeStore/SiteTreeStore';
import ArchiveTreeStore from 'stores/TreeStore/ArchiveTreeStore';
import WidgetTreeStore from 'stores/TreeStore/WidgetTreeStore';
import ContentVersionTreeStore from 'stores/TreeStore/ContentVersionTreeStore';
import TreeStoreType from 'enums/TreeStoreType';

interface Props {
    type: 'main' | 'widgets' | 'versions';
    tree: TreeType;
    treeStore?: TreeStore;
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

@inject('navigationStore', 'editArticleStore', 'treeStore')
@observer
export default class SiteTree extends React.Component<Props> {

    static defaultProps: Pick<Props, DefaultProps> = {
        sbHeightMin: 30,
        sbHeightMax: 850,
        spinnerSize: 30,
    };

    private resolveTree = (): TreeType => {
        const { type, treeStore } = this.props;
        if (type === 'main') {
            return treeStore.resolveMainTreeStore();
        }
        if (type === 'widgets') {
            return treeStore.getWidgetTreeStore();
        }
        if (type === 'versions') {
            return treeStore.getContentVersionTreeStore();
        }
    }

    private handleMajorTreeNode = (e: ITreeElement) => {
        const { navigationStore, editArticleStore, treeStore } = this.props;
        const tree = treeStore.resolveMainTreeStore();
        tree.handleNodeClick(e);
        navigationStore.setDefaultTab(e.isSelected);
        [
            editArticleStore,
            treeStore.getContentVersionTreeStore(),
            treeStore.getWidgetTreeStore(),
        ].forEach((x) => {
            if (tree instanceof SiteTreeStore || tree instanceof ArchiveTreeStore) {
                x.init(tree.selectedNode);
            }
        });
    }

    private handleMinorTreeNode = (e: ITreeElement) => {
        const { tree } = this.props;
        tree.handleNodeClick(e);
        if (!e.isSelected) {
            tree.selectedNode = null;
        }
    }

    private handleNodeClick = (e: ITreeElement) => {
        const { type } = this.props;
        if (type === 'main') {
            this.handleMajorTreeNode(e);
        } else {
            this.handleMinorTreeNode(e);
        }
    }

    render() {
        const { treeStore } = this.props;
        const tree = this.resolveTree();
        const isLoading = treeStore.state === OperationState.PENDING;
        console.log('TREE', tree.tree);
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
