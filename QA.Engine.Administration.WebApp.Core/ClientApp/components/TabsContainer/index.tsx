import * as React from 'react';
import { Card, Tabs, Tab } from '@blueprintjs/core';
import CommonTab from './CommonTab';
import WidgetsTab from './WidgetsTab';

interface Props {

}

export default class TabsContainer extends React.Component<Props> {
    render() {
        return (
            <Card className="tabs-pane">
                <Tabs
                    id="element-view"
                    animate
                >
                    <Tab id="common" title="Common" panel={<CommonTab />} />
                    <Tab id="widgets" title="Widgets" panel={<WidgetsTab />} />
                </Tabs>
            </Card>
        );
    }
}
