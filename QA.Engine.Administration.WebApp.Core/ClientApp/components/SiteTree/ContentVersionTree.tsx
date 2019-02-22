import * as React from 'react';
import { inject, observer } from 'mobx-react';
import { ITreeElement } from 'stores/TreeStore/BaseTreeStore';
import { Card } from '@blueprintjs/core';
import Scrollbars from 'react-custom-scrollbars'; // tslint:disable-line
import TreeStore from 'stores/TreeStore';
import { CustomTree } from 'components/SiteTree/CustomTree';

@observer
class TreeR extends CustomTree {
}

interface Props {
    treeStore?: TreeStore;
}

interface InternalStyle extends JSX.IntrinsicAttributes, React.ClassAttributes<HTMLDivElement>, React.HTMLAttributes<HTMLDivElement> {
}

interface InternalRestProps extends JSX.IntrinsicAttributes, React.ClassAttributes<HTMLDivElement>, React.HTMLAttributes<HTMLDivElement> {
}

@inject('treeStore')
@observer
export default class ContentVersionTree extends React.Component<Props> {

    private handleNodeClick = (e: ITreeElement) => {
        const { treeStore } = this.props;
        const tree = treeStore.getContentVersionsStore();
        tree.handleNodeClick(e);
        if (!e.isSelected) {
            tree.selectedNode = null;
        }
    }

    render() {
        const { treeStore } = this.props;
        const tree = treeStore.getContentVersionsStore();

        return (
            <Card className="tree-pane">
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
                        contents={tree.tree}
                        onNodeClick={this.handleNodeClick}
                    />
                </Scrollbars>
            </Card>
        );
    }
}
