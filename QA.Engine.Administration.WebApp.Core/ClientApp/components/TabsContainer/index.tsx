import * as React from 'react';
import { observer, inject } from 'mobx-react';
import { Card, Tabs, Tab, TabId } from '@blueprintjs/core';
import { TabTypes, TabsState } from 'stores/TabsStore';
import CommonTab from './CommonTab';
import WidgetsTab from './WidgetsTab';

interface Props {
    tabsStore?: TabsState;
}

@inject('tabsStore')
@observer
export default class TabsContainer extends React.Component<Props> {
    private handleChange = (newTabId: TabId & TabTypes) => {
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
                    <Tab id={TabTypes.COMMON} title="Common" panel={<CommonTab data={tabsStore.tabData} />} />
                    <Tab id={TabTypes.WIDGETS} title="Widgets" panel={<WidgetsTab />} />
                </Tabs>
            </Card>
        );
    }
}
