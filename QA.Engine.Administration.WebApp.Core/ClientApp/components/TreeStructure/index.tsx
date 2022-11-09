import * as React from "react";
import { inject, observer } from "mobx-react";
import lodashThrottle from "lodash.throttle";
import { autorun } from "mobx";
import {
    Button,
    Card,
    InputGroup,
    Navbar,
    NavbarDivider,
    NumericInput,
    Spinner,
    Tag,
    ButtonGroup,
    Intent,
} from "@blueprintjs/core";
import { Scrollbars } from "react-custom-scrollbars-2"; // tslint:disable-line
import cn from "classnames"; // tslint:disable-line
import NavigationStore from "stores/NavigationStore";
import OperationState from "enums/OperationState";
import { ITreeElement } from "stores/TreeStore/BaseTreeStore";
import EditArticleStore from "stores/EditArticleStore";
import TreeStore, { TreeStructureType, TreeType } from "stores/TreeStore";
import { CustomTree } from "components/TreeStructure/CustomTree";
import SiteTreeStore from "stores/TreeStore/SiteTreeStore";
import RegionSelect from "components/Select/RegionSelect";
import RegionStore from "stores/RegionStore";
import TextStore from "stores/TextStore";
import Texts from "constants/Texts";
import ArchiveTreeStore from "stores/TreeStore/ArchiveTreeStore";
import WidgetIdSelector from "components/WidgetIdSelector";

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
    shouldScroll: boolean;
    sbHeightMax: number;

    expandCollapseСounter: number;
}

type DefaultProps =
    | "sbHeightMin"
    | "sbHeightDelta"
    | "sbThumbSize"
    | "spinnerSize";

interface InternalStyle
    extends JSX.IntrinsicAttributes,
        React.ClassAttributes<HTMLDivElement>,
        React.HTMLAttributes<HTMLDivElement> {}

interface InternalRestProps
    extends JSX.IntrinsicAttributes,
        React.ClassAttributes<HTMLDivElement>,
        React.HTMLAttributes<HTMLDivElement> {}
@inject(
    "navigationStore",
    "editArticleStore",
    "treeStore",
    "regionStore",
    "textStore"
)
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
            shouldScroll: false,
            sbHeightMax: window.innerHeight - props.sbHeightDelta,
            expandCollapseСounter: 0, // need for rerender Scrollbars component
        };

        autorun(() => {
            const { treeStore, editArticleStore } = props;
            const tree = treeStore.resolveMainTreeStore();
            editArticleStore.init(tree.selectedNode);
        });
    }

    private paginationTimer: number;

    private sbRef = React.createRef<Scrollbars>();

    // need for rerender Scrollbars component
    private updateExpandCollapseСounter() {
        this.setState((state) => ({
            expandCollapseСounter: state.expandCollapseСounter + 1,
        }));
    }

    private handleMajorTreeNode = (e: ITreeElement) => {
        const { navigationStore, treeStore } = this.props;
        const tree = treeStore.resolveMainTreeStore();
        tree.handleNodeClick(e);
        navigationStore.setDefaultTab(e.isSelected);
    };

    private handleMinorTreeNode = (e: ITreeElement) => {
        const { tree } = this.props;
        tree.handleNodeClick(e);
        if (!e.isSelected) {
            tree.selectedNode = null;
        }
    };

    private handleNodeClick = (e: ITreeElement) => {
        const { type, onNodeClick } = this.props;
        if (type === "main") {
            this.handleMajorTreeNode(e);
        } else {
            this.handleMinorTreeNode(e);
        }
        if (onNodeClick != null) {
            onNodeClick(e);
        }
    };

    private handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        if (this.props.type === "main") {
            this.props.navigationStore.resetTab();
        }
        this.props.treeStore
            .resolveTree(this.props.type)
            .search(e.target.value);
    };

    private changeRegion = (e: RegionModel) => {
        const { treeStore, type } = this.props;
        const store = treeStore.resolveMainTreeStore();
        if (type === "main" && store instanceof SiteTreeStore) {
            store.setRegions(e.id);
            store.fetchTree();
        }
    };

    private scrollTo = (y: number) => {
        const { current } = this.sbRef;
        if (current != null) {
            current.scrollTop(y);
        }
    };

    private handleResize = lodashThrottle(() => {
        this.setState({
            sbHeightMax: window.innerHeight - this.props.sbHeightDelta,
        });
    }, 100);

    private handleNextPage = () => {
        const { treeStore } = this.props;
        const tree = treeStore.getArchiveTreeStore();
        tree.handlePagination();
    };

    private handlePrevPage = () => {
        const { treeStore } = this.props;
        const tree = treeStore.getArchiveTreeStore();
        tree.handlePagination(tree.pageIndex - 1);
    };

    private handleLastPage = () => {
        const { treeStore } = this.props;
        const tree = treeStore.getArchiveTreeStore();
        tree.handlePagination(tree.pagesCount);
    };

    private handleFirstPage = () => {
        const { treeStore } = this.props;
        const tree = treeStore.getArchiveTreeStore();
        tree.handlePagination(0);
    };

    private handleInputPage = (val: number) => {
        const { treeStore } = this.props;
        const tree = treeStore.getArchiveTreeStore();
        clearTimeout(this.paginationTimer);
        this.paginationTimer = window.setTimeout(
            () => tree.handlePagination(val),
            500
        );
    };

    private moveClick = async () => {
        const { treeStore } = this.props;
        const tree = treeStore.resolveMainTreeStore();
        if (tree instanceof SiteTreeStore) {
            const model: MoveModel = {
                itemId: tree.moveItemId,
                newParentId: tree.selectedNode.id,
            };
            await treeStore.move(model);
            tree.cancelMoveTree();
        }
    };

    private cancelMoveClick = () => {
        const { navigationStore, treeStore } = this.props;
        const tree = treeStore.resolveMainTreeStore();
        if (tree instanceof SiteTreeStore) {
            navigationStore.setDefaultTab(true);
            tree.cancelMoveTree();
        }
    };

    componentDidMount() {
        window.addEventListener("resize", this.handleResize);
    }

    componentWillUnmount() {
        window.removeEventListener("resize", this.handleResize);
    }

    render() {
        const { treeStore, regionStore, textStore, type } = this.props;
        const tree = treeStore.resolveTree(type);
        const isLoading = treeStore.state === OperationState.PENDING;
        const useRegions =
            type === "main" &&
            tree instanceof SiteTreeStore &&
            regionStore.useRegions;
        const usePagination =
            tree instanceof ArchiveTreeStore &&
            tree.page !== null &&
            !tree.searchActive;
        const regions =
            regionStore.regions != null && regionStore.regions.length > 0
                ? [{ id: null, title: "(No selection)" } as RegionModel].concat(
                      regionStore.regions
                  )
                : [];
        const isMoveTreeMode =
            tree instanceof SiteTreeStore ? tree.moveTreeMode : false;
        const isNewParentSelected =
            tree instanceof SiteTreeStore && tree.treeElement
                ? !tree.treeElement.isSelected
                : false;
        const scrollNodeId = tree.getNodeToScroll();
        if (scrollNodeId !== null) {
            setTimeout(() => {
                this.scrollTo(tree.nodeCords.get(scrollNodeId));
            }, 0);
        }

        return (
            <Card
                className={cn("tree-pane", this.props.className)}
                style={isMoveTreeMode ? { width: "100%" } : null}
            >
                <Navbar className="tree-navbar">
                    <InputGroup
                        leftIcon="search"
                        type="search"
                        onChange={this.handleInputChange}
                        value={tree.query}
                        placeholder="Title/Alias/ID"
                    />
                    <NavbarDivider />
                    <WidgetIdSelector
                        tree={tree}
                        textStore={this.props.textStore}
                    />
                    {useRegions && !isMoveTreeMode && (
                        <React.Fragment>
                            <NavbarDivider
                                className={cn({ hidden: tree.searchActive })}
                            />
                            <RegionSelect
                                items={regions}
                                filterable
                                onChange={this.changeRegion}
                                className={cn({ hidden: tree.searchActive })}
                            />
                        </React.Fragment>
                    )}
                    {isMoveTreeMode && (
                        <React.Fragment>
                            <NavbarDivider />
                            <ButtonGroup
                                className="dialog-button-group"
                                style={{ justifyContent: "left" }}
                            >
                                <Button
                                    text={
                                        textStore.texts[Texts.popupMoveButton]
                                    }
                                    icon="move"
                                    onClick={this.moveClick}
                                    intent={Intent.SUCCESS}
                                    disabled={isNewParentSelected}
                                />
                                <Button
                                    text={
                                        textStore.texts[Texts.popupCancelButton]
                                    }
                                    icon="undo"
                                    onClick={this.cancelMoveClick}
                                />
                            </ButtonGroup>
                        </React.Fragment>
                    )}
                </Navbar>
                {isLoading ? (
                    <Spinner size={this.props.spinnerSize} />
                ) : (
                    <React.Fragment>
                        <Scrollbars
                            ref={this.sbRef}
                            hideTracksWhenNotNeeded
                            autoHeight
                            autoHeightMin={this.props.sbHeightMin}
                            autoHeightMax={
                                usePagination
                                    ? this.state.sbHeightMax - 30
                                    : this.state.sbHeightMax
                            }
                            thumbMinSize={this.props.sbThumbSize}
                            renderTrackVertical={(
                                style: InternalStyle,
                                ...props: InternalRestProps[]
                            ) => <div className="track-vertical" {...props} />}
                            renderThumbVertical={(
                                style: InternalStyle,
                                ...props: InternalRestProps[]
                            ) => <div className="thumb-vertical" {...props} />}
                        >
                            <CustomTree
                                className="tree"
                                contents={
                                    tree.searchActive
                                        ? tree.searchedTree
                                        : tree.tree
                                }
                                tree={tree}
                                onNodeCollapse={tree.handleNodeCollapse}
                                onNodeExpand={tree.handleNodeExpand}
                                onNodeClick={this.handleNodeClick}
                                updateScroll={() =>
                                    this.updateExpandCollapseСounter()
                                }
                            />
                        </Scrollbars>
                        {usePagination && (
                            <div className="pagination">
                                <Button
                                    icon="arrow-left"
                                    minimal
                                    onClick={this.handlePrevPage}
                                    disabled={tree.page === 1}
                                />
                                <Tag
                                    className="left"
                                    interactive
                                    onClick={this.handleFirstPage}
                                >
                                    1
                                </Tag>
                                <NumericInput
                                    max={tree.pagesCount + 1}
                                    min={1}
                                    buttonPosition="none"
                                    value={`${tree.page}`}
                                    clampValueOnBlur
                                    onValueChange={this.handleInputPage}
                                />
                                <Tag
                                    className="right"
                                    interactive
                                    onClick={this.handleLastPage}
                                >
                                    {tree.pagesCount + 1}
                                </Tag>
                                <Button
                                    icon="arrow-right"
                                    minimal
                                    onClick={this.handleNextPage}
                                    disabled={
                                        tree.pageIndex === tree.pagesCount
                                    }
                                />
                            </div>
                        )}
                    </React.Fragment>
                )}
            </Card>
        );
    }
}
