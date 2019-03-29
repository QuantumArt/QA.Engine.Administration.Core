import * as React from 'react';
import { inject, observer } from 'mobx-react';
import { Card, Tab, TabId, Tabs } from '@blueprintjs/core';
import NavigationStore, { Pages, TabTypes } from 'stores/NavigationStore';
import CommonTab from './CommonTab';
import WidgetTab from './WidgetTab';
import ContentVersionTab from './ContentVersionTab';
import TextStore from 'stores/TextStore';
import Texts from 'constants/Texts';
import TreeStore from 'stores/TreeStore';
import { autorun, when } from 'mobx';

interface Props {
    navigationStore?: NavigationStore;
    textStore?: TextStore;
    treeStore?: TreeStore;
}

@inject('navigationStore', 'textStore', 'treeStore')
@observer
export default class TabsContainer extends React.Component<Props> {

    constructor(props: Props) {
        super(props);
        autorun(() => {
            const { navigationStore, treeStore } = props;
            const tree = treeStore.resolveMainTreeStore();
            switch (navigationStore.currentTab) {
                case TabTypes.WIDGETS:
                    treeStore.getWidgetTreeStore().init(tree.selectedNode);
                    break;
                case TabTypes.CONTENT_VERSIONS:
                    treeStore.getContentVersionTreeStore().init(tree.selectedNode);
                    break;
            }
        });
    }

    private handleChange = (newTabId: TabId & TabTypes) => {
        this.props.navigationStore.changeTab(newTabId);
    }

    render() {
        const { navigationStore, textStore } = this.props;
        return (
            <Card className="tabs-pane">
                <Tabs
                    selectedTabId={navigationStore.currentTab}
                    onChange={this.handleChange}
                    id="element-view"
                    animate
                >
                    {navigationStore.currentPage === Pages.SITEMAP ?
                        [
                            <Tab key={TabTypes.COMMON} id={TabTypes.COMMON} title={textStore.texts[Texts.commonTab]} panel={<CommonTab />} />,
                            <Tab key={TabTypes.WIDGETS} id={TabTypes.WIDGETS} title={textStore.texts[Texts.widgetTab]} panel={<WidgetTab />} />,
                            <Tab key={TabTypes.CONTENT_VERSIONS} id={TabTypes.CONTENT_VERSIONS} title={textStore.texts[Texts.contentVersionTab]} panel={<ContentVersionTab />} />,
                        ] :
                        [
                            <Tab key={TabTypes.COMMON} id={TabTypes.COMMON} title={textStore.texts[Texts.commonTab]} panel={<CommonTab />} />,
                        ]
                    }
                </Tabs>
            </Card>
        );
    }
}
