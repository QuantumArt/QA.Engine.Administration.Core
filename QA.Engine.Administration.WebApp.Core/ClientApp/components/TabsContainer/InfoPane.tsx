import * as React from 'react';
import { observer, inject } from 'mobx-react';
import { Card, Checkbox, H5 } from '@blueprintjs/core';
import Texts from 'constants/Texts';
import TreeStore from 'stores/TreeStore';
import TextStore from 'stores/TextStore';

interface Props {
    type: 'widgets' | 'versions';
    treeStore?: TreeStore;
    textStore?: TextStore;
}

const InfoPane = inject('treeStore', 'textStore')(observer((props: Props) => {
    const { type, treeStore, textStore } = props;
    let tree;
    if (type === 'widgets') {
        tree = treeStore.getWidgetTreeStore();
    } else if (type === 'versions') {
        tree = treeStore.getContentVersionTreeStore();
    }
    const selectedNode = tree.selectedNode;

    if (selectedNode === null) {
        return null;
    }

    return (
        <Card className="tab-aside">
            <div className="tab-entity">
                <H5>ID</H5>
                <p>{selectedNode.id}</p>
            </div>
            <div className="tab-entity">
                <H5>{textStore.texts[Texts.title]}</H5>
                <p>{selectedNode.title}</p>
            </div>
            <div className="tab-entity">
                <H5>{textStore.texts[Texts.typeName]}</H5>
                <p>{selectedNode.discriminatorTitle}</p>
            </div>
            <div className="tab-entity">
                <H5>{textStore.texts[Texts.alias]}</H5>
                <p>{selectedNode.alias}</p>
            </div>
            <div className="tab-entity">
                <Checkbox checked={selectedNode.published} disabled={true}>{textStore.texts[Texts.published]}</Checkbox>
            </div>
            <div className="tab-entity">
                <Checkbox checked={selectedNode.visible} disabled={true}>{textStore.texts[Texts.visible]}</Checkbox>
            </div>
        </Card>
    );
}));

export default InfoPane;
