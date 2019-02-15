import * as React from 'react';
import { inject, observer } from 'mobx-react';
import { Card, Tree, Spinner } from '@blueprintjs/core';
import Scrollbars from 'react-custom-scrollbars';
import { SiteTreeState } from 'stores/SiteTreeStore';
import OperationState from 'enums/OperationState';

@observer
class TreeR extends Tree {}

interface Props {
    siteTreeStore?: SiteTreeState;
}

// TODO: Finish scrollbar style
interface InternalStyle extends JSX.IntrinsicAttributes, React.ClassAttributes<HTMLDivElement>, React.HTMLAttributes<HTMLDivElement> {}
interface InternalRestProps extends JSX.IntrinsicAttributes, React.ClassAttributes<HTMLDivElement>, React.HTMLAttributes<HTMLDivElement> {}

@inject('siteTreeStore')
@observer
export default class SiteTree extends React.Component<Props> {
    render() {
        const { siteTreeStore } = this.props;
        siteTreeStore.loadData();
        const isLoading = siteTreeStore.treeState === OperationState.NONE || siteTreeStore.treeState === OperationState.PENDING;
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
                            contents={siteTreeStore.tree}
                            className="site-tree"
                            onNodeCollapse={siteTreeStore.handleNodeCollapse}
                            onNodeExpand={siteTreeStore.handleNodeExpand}
                            onNodeClick={siteTreeStore.handleNodeClick}
                        />
                    </Scrollbars>
                }
            </Card>
        );
    }
}
