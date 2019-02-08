import * as React from 'react';
import { observer, inject } from 'mobx-react';
import { Card, Tabs, Tab, TabId } from '@blueprintjs/core';
import TabsStore, { TabsState } from 'stores/TabsStore';
import CommonTab from './CommonTab';
import WidgetsTab from './WidgetsTab';

interface Props {
    tabsStore?: TabsStore;
}

@inject('tabsStore')
@observer
export default class TabsContainer extends React.Component<Props> {
    private handleChange = (newTabId: TabId & TabsState) => {
        const { tabsStore } = this.props;
        if (tabsStore.tabData !== null) {
            tabsStore.setTab(newTabId);
        }
    }

    render() {
        const { tabsStore } = this.props;
        return (
            <Card className="tabs-pane">
                <Tabs
                    selectedTabId={tabsStore.currentTab}
                    onChange={this.handleChange}
                    id="element-view"
                    animate
                >
                    <Tab id={TabsState.COMMON} title="Common" panel={<CommonTab data={tabsStore.tabData} />} />
                    <Tab id={TabsState.WIDGETS} title="Widgets" panel={<WidgetsTab />} />
                </Tabs>
            </Card>
        );
    }
}
