import * as React from 'react';
import { inject, observer } from 'mobx-react';
import lodashThrottle from 'lodash.throttle';
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
import TreeStore, { TreeStructureType, TreeType } from 'stores/TreeStore';
import { CustomTree } from 'components/TreeStructure/CustomTree';
import SiteTreeStore from 'stores/TreeStore/SiteTreeStore';
import RegionSelect from 'components/Select/RegionSelect';
import RegionStore from 'stores/RegionStore';
import ArchiveTreeStore from 'stores/TreeStore/ArchiveTreeStore';
import TextStore from 'stores/TextStore';
import Texts from 'constants/Texts';

interface Props {
    type: TreeStructureType;
    tree: TreeType;
    treeStore?: TreeStore;
    navigationStore?: NavigationStore;
    editArticleStore?: EditArticleStore;
    textStore?: TextStore;
    regionStore?: RegionStore;
    sbHeightMin?: number;
    sbHeightDelta?: number;
    sbThumbSize?: number;
    spinnerSize?: number;
    className?: string;
    onNodeClick?: (e: ITreeElement) => void;
}

interface State {
    currentNode: PageModel | ArchiveModel;
    shouldScroll: boolean;
    sbHeightMax: number;
}

type DefaultProps = 'sbHeightMin' | 'sbHeightDelta' | 'sbThumbSize' | 'spinnerSize';

interface InternalStyle extends JSX.IntrinsicAttributes, React.ClassAttributes<HTMLDivElement>, React.HTMLAttributes<HTMLDivElement> {
}

interface InternalRestProps extends JSX.IntrinsicAttributes, React.ClassAttributes<HTMLDivElement>, React.HTMLAttributes<HTMLDivElement> {
}

@inject('navigationStore', 'editArticleStore', 'treeStore', 'regionStore', 'textStore')
@observer
export default class SiteTree extends React.Component<Props, State> {

    static defaultProps: Pick<Props, DefaultProps> = {
        sbHeightMin: 30,
        sbHeightDelta: 180,
        spinnerSize: 30,
    };

    constructor(props: Props) {
        super(props);

        this.state = {
            currentNode: null,
            shouldScroll: false,
            sbHeightMax: window.innerHeight - props.sbHeightDelta,
        };

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
                this.setState({ currentNode: tree.selectedNode });
            }
            if (selectedNodeId !== currentNodeId && type === 'main' && tree instanceof ArchiveTreeStore) {
                editArticleStore.init(tree.selectedNode);
            }
            return false;
        });
    }

    private sbRef = React.createRef<Scrollbars>();

    private handleMajorTreeNode = (e: ITreeElement) => {
        const { navigationStore, treeStore } = this.props;
        const tree = treeStore.resolveMainTreeStore();
        tree.handleNodeClick(e);
        navigationStore.setDefaultTab(e.isSelected);
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
        if (this.props.type === 'main') {
            this.props.navigationStore.resetTab();
        }
        this.props.treeStore.resolveTree(this.props.type).search(e.target.value);
    }

    private changeRegion = (e: RegionModel) => {
        const { treeStore, type } = this.props;
        const store = treeStore.resolveMainTreeStore();
        if (type === 'main' && store instanceof SiteTreeStore) {
            store.setRegions(e.id);
            store.fetchTree();
        }
    }

    private scrollTo = (y: number) => {
        const { current } = this.sbRef;
        if (current != null) {
            current.scrollTop(y);
        }
    }

    private handleResize = lodashThrottle(
        () => {
            this.setState({
                sbHeightMax: window.innerHeight - this.props.sbHeightDelta,
            });
        },
        100,
    );

    componentDidMount() {
        window.addEventListener('resize', this.handleResize);
    }

    componentWillUnmount() {
        window.removeEventListener('resize', this.handleResize);
    }

    render() {
        const { treeStore, regionStore, textStore, type } = this.props;
        const tree = treeStore.resolveTree(type);
        const isLoading = treeStore.state === OperationState.PENDING;
        const useRegions = type === 'main' && tree instanceof SiteTreeStore && regionStore.useRegions;
        const regions = regionStore.regions != null && regionStore.regions.length > 0
            ? [{ id: null, title: '(No selection)' } as RegionModel].concat(regionStore.regions)
            : [];
        const scrollNodeId = tree.getNodeToScroll();
        if (scrollNodeId !== null) {
            setTimeout(
                () => {
                    this.scrollTo(tree.nodeCords.get(scrollNodeId));
                },
                0,
            );
        }

        return (
            <Card className={cn('tree-pane', this.props.className)}>
                <Navbar className="tree-navbar">
                    <InputGroup
                        leftIcon="search"
                        type="search"
                        onChange={this.handleInputChange}
                        value={tree.query}
                        placeholder="Title/Alias/ID"
                    />
                    <NavbarDivider />
                    <Switch
                        inline
                        label={textStore.texts[Texts.showID]}
                        className="tree-switch"
                        checked={tree.showIDs}
                        onChange={tree.toggleIDs}
                    />
                    {tree.searchActive &&
                        <Switch
                            inline
                            label={textStore.texts[Texts.showPath]}
                            className="tree-switch"
                            checked={tree.showPath}
                            onChange={tree.togglePath}
                        />
                    }
                    {useRegions &&
                        <React.Fragment>
                            <NavbarDivider className={cn({ hidden: tree.searchActive })} />
                            <RegionSelect
                                items={regions}
                                filterable
                                onChange={this.changeRegion}
                                className={cn({ hidden: tree.searchActive })}
                            />
                        </React.Fragment>
                    }
                </Navbar>
                {isLoading ?
                    <Spinner size={this.props.spinnerSize}/> :
                    <Scrollbars
                        ref={this.sbRef}
                        hideTracksWhenNotNeeded
                        autoHeight
                        autoHide
                        autoHeightMin={this.props.sbHeightMin}
                        autoHeightMax={this.state.sbHeightMax}
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
                        <CustomTree
                            className="tree"
                            contents={tree.searchActive ? tree.searchedTree : tree.tree}
                            tree={tree}
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
