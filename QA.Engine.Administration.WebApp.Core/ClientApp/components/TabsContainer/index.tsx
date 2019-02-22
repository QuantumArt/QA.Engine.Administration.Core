import * as React from 'react';
import { inject, observer } from 'mobx-react';
import { Card, Tab, TabId, Tabs } from '@blueprintjs/core';
import NavigationStore, { Pages, TabTypes } from 'stores/NavigationStore';
import CommonTab from './CommonTab';
import WidgetTab from './WidgetTab';
import ContentVersionTab from './ContentVersionTab';

interface Props {
    navigationStore?: NavigationStore;
}

@inject('navigationStore')
@observer
export default class TabsContainer extends React.Component<Props> {

    private handleChange = (newTabId: TabId & TabTypes) => {
        const { navigationStore } = this.props;
        navigationStore.changeTab(newTabId);
    }

    render() {
        const { navigationStore } = this.props;
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
                            <Tab key={TabTypes.COMMON} id={TabTypes.COMMON} title="Common" panel={<CommonTab />} />,
                            <Tab key={TabTypes.WIDGETS} id={TabTypes.WIDGETS} title="Widgets" panel={<WidgetTab />} />,
                            <Tab key={TabTypes.CONTENT_VERSIONS} id={TabTypes.CONTENT_VERSIONS} title="Content versions" panel={<ContentVersionTab />} />,
                        ] :
                        [
                            <Tab key={TabTypes.COMMON} id={TabTypes.COMMON} title="Common" panel={<CommonTab />} />,
                        ]
                    }
                </Tabs>
            </Card>
        );
    }
}
