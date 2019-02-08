import * as React from 'react';
import { inject, observer } from 'mobx-react';
import { Card, Tree, Spinner } from '@blueprintjs/core';
import { SiteTreeState } from 'stores/SiteTreeStore';
import { ArchiveState } from 'stores/ArchiveStore';
import TreeState from 'enums/TreeState';

@observer
class TreeR extends Tree {}

interface Props {
    siteTreeStore?: SiteTreeState;
    archiveStore?: ArchiveState;
}

@inject('siteTreeStore', 'archiveStore')
@observer
export default class SiteTree extends React.Component<Props> {
    render() {
        const { siteTreeStore } = this.props;
        const isLoading = siteTreeStore.siteTreeState === TreeState.NONE || siteTreeStore.siteTreeState === TreeState.PENDING;
        return (
            <Card className="tree-pane">
                {isLoading ?
                    <Spinner size={30} /> :
                    <TreeR
                        contents={siteTreeStore.tree}
                        className="site-tree"
                        onNodeCollapse={siteTreeStore.handleNodeCollapse}
                        onNodeExpand={siteTreeStore.handleNodeExpand}
                        onNodeClick={siteTreeStore.handleNodeClick}
                    />
                }
            </Card>
        );
    }
}
