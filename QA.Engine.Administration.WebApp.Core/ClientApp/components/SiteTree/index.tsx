import * as React from 'react';
import { inject, observer } from 'mobx-react';
import { Tree, Card } from '@blueprintjs/core';
import SiteTreeStore from 'stores/SiteTreeStore';
import ArchiveStore from 'stores/ArchiveStore';
import TreeState from 'enums/TreeState';

@observer
class TreeR extends Tree {}

interface Props {
    siteTreeStore?: SiteTreeStore;
    archiveStore?: ArchiveStore;
}

@inject('siteTreeStore', 'archiveStore')
@observer
export default class SiteTree extends React.Component<Props> {
    render() {
        const { siteTreeStore } = this.props;
        console.log('TreeState', TreeState);
        return (
            <Card className="tree-pane">
                <TreeR
                    contents={siteTreeStore.tree}
                    className="site-tree"
                    onNodeCollapse={siteTreeStore.handleNodeCollapse}
                    onNodeExpand={siteTreeStore.handleNodeExpand}
                    onNodeClick={siteTreeStore.handleNodeClick}
                />
            </Card>
        );
    }
}
