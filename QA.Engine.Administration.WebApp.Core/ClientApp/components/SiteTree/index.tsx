import * as React from 'react';
import { inject, observer } from 'mobx-react';
import { Icon, ITreeNode, Tooltip, Tree, Card } from '@blueprintjs/core';
import UIStore from 'stores/UIStore';

interface Props {
    uiStore?: UIStore;
}

@inject('uiStore')
@observer
export default class SiteTree extends React.Component<Props> {

    handleNodeClick = (nodeData: ITreeNode, _nodePath: number[], e: React.MouseEvent<HTMLElement>) => {
        console.log(nodeData, _nodePath);
    }

    componentDidMount(): void {
        this.props.uiStore.fetchSiteTree();
    }

    render() {
        const { uiStore } = this.props;
        return (
            <Card>
                <Tree
                    contents={testnodes}
                    onNodeClick={this.handleNodeClick}
                />
            </Card>
        );
    }
}

const testnodes: ITreeNode[] = [
    {
        id: 0,
        hasCaret: true,
        icon: 'application',
        label: 'Root',
        isExpanded: true,
        childNodes: [
            {
                id: 1,
                icon: 'document',
                label: 'start_page',
                secondaryLabel: (
                    <Tooltip content="An eye!">
                        <Icon icon="eye-open"/>
                    </Tooltip>
                ),
            },
        ],
    },
];
