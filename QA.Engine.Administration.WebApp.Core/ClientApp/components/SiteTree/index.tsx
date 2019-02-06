import * as React from 'react';
import { inject, observer } from 'mobx-react';
import { Tree, Card } from '@blueprintjs/core';
import SiteTreeStore from 'stores/SiteTreeStore';

@observer
class TreeR extends Tree {}

interface Props {
    siteTreeStore?: SiteTreeStore;
}

@inject('siteTreeStore')
@observer
export default class SiteTree extends React.Component<Props> {
    componentDidMount(): void {
        this.props.siteTreeStore.fetchSiteTree();
    }

    render() {
        const { siteTreeStore } = this.props;
        return (
            <Card>
                <TreeR
                    contents={siteTreeStore.tree}
                    onNodeCollapse={siteTreeStore.handleNodeCollapse}
                    onNodeExpand={siteTreeStore.handleNodeExpand}
                />
            </Card>
        );
    }
}
