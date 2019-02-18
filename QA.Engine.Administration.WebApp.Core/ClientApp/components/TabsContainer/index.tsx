import * as React from 'react';
import { inject, observer } from 'mobx-react';
import { Card, Tab, TabId, Tabs } from '@blueprintjs/core';
import { TabsState, TabTypes } from 'stores/TabsStore';
import { NavigationState, Pages } from 'stores/NavigationStore';
import CommonTab from './CommonTab';
import WidgetsTab from './WidgetsTab';

interface Props {
    tabsStore?: TabsState;
    navigationStore?: NavigationState;
}

@inject('tabsStore', 'navigationStore')
@observer
export default class TabsContainer extends React.Component<Props> {
    private handleChange = (newTabId: TabId & TabTypes) => {
        const { tabsStore } = this.props;
        if (tabsStore.tabData !== null) {
            tabsStore.setTab(newTabId);
        }
    }

    render() {
        const { tabsStore, navigationStore } = this.props;
        return (
            <Card className="tabs-pane">
                <Tabs
                    selectedTabId={tabsStore.currentTab}
                    onChange={this.handleChange}
                    id="element-view"
                    animate
                >
                    {navigationStore.currentPage === Pages.SITEMAP ?
                        [
                            <Tab key={TabTypes.COMMON} id={TabTypes.COMMON} title="Common" panel={<CommonTab data={tabsStore.tabData}/>} />,
                            <Tab key={TabTypes.WIDGETS} id={TabTypes.WIDGETS} title="Widgets" panel={<WidgetsTab />} />,
                        ] :
                        [
                            <Tab key={TabTypes.COMMON} id={TabTypes.COMMON} title="Common" panel={<CommonTab data={tabsStore.tabData}/>} />,
                        ]
                    }
                </Tabs>
            </Card>
        );
    }
}
