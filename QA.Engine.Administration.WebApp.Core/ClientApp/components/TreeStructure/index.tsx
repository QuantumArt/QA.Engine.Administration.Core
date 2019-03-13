import * as React from 'react';
import { inject, observer } from 'mobx-react';
import { when } from 'mobx';
import {
    Card,
    InputGroup,
    Navbar,
    NavbarDivider,
    Spinner, Switch,
} from '@blueprintjs/core';
import Scrollbars from 'react-custom-scrollbars'; // tslint:disable-line
import cn from 'classnames'; // tslint:disable-line
import NavigationStore  from 'stores/NavigationStore';
import OperationState from 'enums/OperationState';
import { ITreeElement } from 'stores/TreeStore/BaseTreeStore';
import EditArticleStore from 'stores/EditArticleStore';
import TreeStore, { TreeType } from 'stores/TreeStore';
import { CustomTree } from 'components/TreeStructure/CustomTree';
import SiteTreeStore from 'stores/TreeStore/SiteTreeStore';
import RegionSelect from 'components/Select/RegionSelect';
import RegionStore from 'stores/RegionStore';
import ArchiveTreeStore from 'stores/TreeStore/ArchiveTreeStore';

interface Props {
    type: 'main' | 'widgets' | 'versions' | 'move';
    tree: TreeType;
    treeStore?: TreeStore;
    navigationStore?: NavigationStore;
    editArticleStore?: EditArticleStore;
    regionStore?: RegionStore;
    sbHeightMin?: number;
    sbHeightMax?: number;
    sbThumbSize?: number;
    spinnerSize?: number;
    className?: string;
    onNodeClick?: (e: ITreeElement) => void;
}

interface State {
    currentNode: PageModel | ArchiveModel;
}

type DefaultProps = 'sbHeightMin' | 'sbHeightMax' | 'sbThumbSize' | 'spinnerSize';

interface InternalStyle extends JSX.IntrinsicAttributes, React.ClassAttributes<HTMLDivElement>, React.HTMLAttributes<HTMLDivElement> {
}

interface InternalRestProps extends JSX.IntrinsicAttributes, React.ClassAttributes<HTMLDivElement>, React.HTMLAttributes<HTMLDivElement> {
}

@observer
class TreeR extends CustomTree {
}

@inject('navigationStore', 'editArticleStore', 'treeStore', 'regionStore')
@observer
export default class SiteTree extends React.Component<Props, State> {

    constructor(props: Props) {
        super(props);

        this.state = { currentNode: null };

        when(() => {
            if (this.props == null || this.state == null) {
                return false;
            }
            const { editArticleStore, treeStore, type } = this.props;
            const { currentNode } = this.state;

            const tree = treeStore.resolveMainTreeStore();
            const currentNodeId = currentNode == null ? null : currentNode.id;
            const selectedNodeId = tree.selectedNode == null ? null : tree.selectedNode.id;

            if (selectedNodeId !== currentNodeId && type === 'main' && tree instanceof SiteTreeStore) {
                [
                    editArticleStore,
                    treeStore.getContentVersionTreeStore(),
                    treeStore.getWidgetTreeStore(),
                ].forEach((x) => {
                    x.init(tree.selectedNode);
                });
                treeStore.getMoveTreeStore().init(tree.selectedNode, tree.origTree);
            }
            if (selectedNodeId !== currentNodeId && type === 'main' && tree instanceof ArchiveTreeStore) {
                editArticleStore.init(tree.selectedNode);
            }

            return false;
        });
    }

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
        if (type === 'move') {
            return treeStore.getMoveTreeStore();
        }
    }

    private handleMajorTreeNode = (e: ITreeElement) => {
        const { navigationStore, treeStore } = this.props;
        const tree = treeStore.resolveMainTreeStore();
        tree.handleNodeClick(e);
        navigationStore.setDefaultTab(e.isSelected);
        this.setState({ currentNode: tree.selectedNode });
    }

    private handleMinorTreeNode = (e: ITreeElement) => {
        const { tree } = this.props;
        tree.handleNodeClick(e);
        if (!e.isSelected) {
            tree.selectedNode = null;
        }
    }

    private handleNodeClick = (e: ITreeElement) => {
        const { type, onNodeClick } = this.props;
        if (type === 'main') {
            this.handleMajorTreeNode(e);
        } else {
            this.handleMinorTreeNode(e);
        }
        if (onNodeClick != null) {
            onNodeClick(e);
        }
    }

    private handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const tree = this.resolveTree();
        tree.search(e.target.value);
    }

    private changeRegion = (e: RegionModel) => {
        const { treeStore, type } = this.props;
        const store = treeStore.resolveMainTreeStore();
        if (type === 'main' && store instanceof SiteTreeStore) {
            store.setRegions(e.id);
            store.fetchTree();
        }
    }

    render() {
        const { treeStore, regionStore, type } = this.props;
        const tree = this.resolveTree();
        const isLoading = treeStore.state === OperationState.PENDING;
        const useRegions = type === 'main' && tree instanceof SiteTreeStore && regionStore.useRegions;
        const regions = regionStore.regions != null && regionStore.regions.length > 0
            ? [{ id: null, title: '(No selection)' } as RegionModel].concat(regionStore.regions)
            : [];

        return (
            <Card className={cn('tree-pane', this.props.className)}>
                <Navbar className="tree-navbar">
                    <InputGroup
                        leftIcon="search"
                        type="search"
                        onChange={this.handleInputChange}
                        value={tree.query}
                    />
                    <NavbarDivider />
                    <Switch
                        inline
                        label="Show IDs"
                        className="tree-switch"
                        alignIndicator="right"
                        checked={tree.showIDs}
                        onChange={tree.toggleIDs}
                    />
                    {useRegions &&
                        <React.Fragment>
                            <NavbarDivider />
                            <RegionSelect items={regions} filterable onChange={this.changeRegion} />
                        </React.Fragment>
                    }
                </Navbar>
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
                            contents={tree.searchActive ? tree.searchedTree : tree.tree}
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
