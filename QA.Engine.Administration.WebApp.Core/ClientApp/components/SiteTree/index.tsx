import * as React from 'react';
import { inject, observer } from 'mobx-react';
import { Card, Spinner, Tree } from '@blueprintjs/core';
import Scrollbars from 'react-custom-scrollbars';
import { SiteTreeState } from 'stores/SiteTreeStore';
import { ArchiveState } from 'stores/ArchiveStore';
import { NavigationState, Pages } from 'stores/NavigationStore';
import OperationState from 'enums/OperationState';

@observer
class TreeR extends Tree {
}

interface Props {
    siteTreeStore?: SiteTreeState;
    archiveStore?: ArchiveState;
    navigationStore?: NavigationState;
}

interface InternalStyle extends JSX.IntrinsicAttributes, React.ClassAttributes<HTMLDivElement>, React.HTMLAttributes<HTMLDivElement> {
}

interface InternalRestProps extends JSX.IntrinsicAttributes, React.ClassAttributes<HTMLDivElement>, React.HTMLAttributes<HTMLDivElement> {
}

@inject('siteTreeStore', 'archiveStore', 'navigationStore')
@observer
export default class SiteTree extends React.Component<Props> {
    private checkLoading = (): boolean => {
        const { siteTreeStore, archiveStore, navigationStore } = this.props;
        if (navigationStore.currentPage === Pages.SITEMAP) {
            return siteTreeStore.treeState === OperationState.NONE || siteTreeStore.treeState === OperationState.PENDING;
        }
        return archiveStore.treeState === OperationState.NONE || archiveStore.treeState === OperationState.PENDING;
    }

    render() {
        const { siteTreeStore, archiveStore, navigationStore } = this.props;
        const isSiteTree = navigationStore.currentPage === Pages.SITEMAP;
        const isLoading = this.checkLoading();

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
                            contents={isSiteTree ? siteTreeStore.tree : archiveStore.tree}
                            onNodeCollapse={isSiteTree ? siteTreeStore.handleNodeCollapse : archiveStore.handleNodeCollapse}
                            onNodeExpand={isSiteTree ? siteTreeStore.handleNodeExpand : archiveStore.handleNodeExpand}
                            onNodeClick={isSiteTree ? siteTreeStore.handleNodeClick : archiveStore.handleNodeClick}
                        />
                    </Scrollbars>
                }
            </Card>
        );
    }
}
