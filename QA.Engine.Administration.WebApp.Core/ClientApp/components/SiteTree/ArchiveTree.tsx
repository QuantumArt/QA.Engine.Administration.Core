import * as React from 'react';
import { inject, observer } from 'mobx-react';
import { Card, Tree, Spinner } from '@blueprintjs/core';
import Scrollbars from 'react-custom-scrollbars';
import { SiteTreeState } from 'stores/SiteTreeStore';
import { ArchiveState } from 'stores/ArchiveStore';
import OperationState from 'enums/OperationState';

@observer
class TreeR extends Tree {}

interface Props {
    archiveStore?: ArchiveState;
}

// TODO: Finish scrollbar style
interface InternalStyle extends JSX.IntrinsicAttributes, React.ClassAttributes<HTMLDivElement>, React.HTMLAttributes<HTMLDivElement> {}
interface InternalRestProps extends JSX.IntrinsicAttributes, React.ClassAttributes<HTMLDivElement>, React.HTMLAttributes<HTMLDivElement> {}

@inject('archiveStore')
@observer
export default class ArchiveTree extends React.Component<Props> {
    render() {
        const { archiveStore } = this.props;
        archiveStore.loadData();
        const isLoading = archiveStore.treeState === OperationState.NONE || archiveStore.treeState === OperationState.PENDING;
        return (
            <Card className="tree-pane">
                {isLoading ?
                    <Spinner size={30} /> :
                    <Scrollbars
                        autoHeight
                        autoHide
                        autoHeightMin={30}
                        autoHeightMax={855}
                        renderTrackHorizontal={(style: InternalStyle, ...props: InternalRestProps[]) =>
                            <div
                                {...props}
                                style={{ ...style }}
                                className="track-vertical"
                            />
                        }
                        renderThumbHorizontal={(style: InternalStyle, ...props: InternalRestProps[]) =>
                            <div
                                {...props}
                                style={{ ...style }}
                                className="thumb-vertical"
                            />
                        }
                    >
                        <TreeR
                            contents={archiveStore.tree}
                            className="site-tree"
                            onNodeCollapse={archiveStore.handleNodeCollapse}
                            onNodeExpand={archiveStore.handleNodeExpand}
                            onNodeClick={archiveStore.handleNodeClick}
                        />
                    </Scrollbars>
                }
            </Card>
        );
    }
}
