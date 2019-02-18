import * as React from 'react';
import { Provider } from 'mobx-react';
import DevTools from 'mobx-react-devtools'; // tslint:disable-line
import { hot } from 'react-hot-loader';
import { Alignment, Button, Classes, Navbar, NavbarGroup, NavbarHeading } from '@blueprintjs/core';
import '@blueprintjs/core/lib/css/blueprint.css';
import '@blueprintjs/icons/lib/css/blueprint-icons.css';
import 'normalize.css/normalize.css';

import 'assets/style.css';
import SiteTree from 'components/SiteTree';
import ArchiveTree from 'components/SiteTree/ArchiveTree';
import TabsContainer from 'components/TabsContainer';
import SiteTreeStore from 'stores/SiteTreeStore';
import TabsStore from 'stores/TabsStore';
import ArchiveStore from 'stores/ArchiveStore';
import QpIntegrationStore from 'stores/QpIntegrationStore';
import ExtensionFieldsStore from 'stores/ExtensionFieldsStore';
import Popup from 'components/Popup/popup';
import PopupStore from 'stores/PopupStore';
import AddPopup from 'components/Popup/add';

const app = hot(module)(() => (
    <Provider
        siteTreeStore={SiteTreeStore}
        archiveStore={ArchiveStore}
        tabsStore={TabsStore}
        qpIntegrationStore={QpIntegrationStore}
        extensionFieldsStore={ExtensionFieldsStore}
        popupStore={PopupStore}
    >
        <div className="layout">
            <Navbar fixedToTop>
                <NavbarGroup align={Alignment.LEFT}>
                    <NavbarHeading>Manage Site</NavbarHeading>
                    <Button className={Classes.MINIMAL} icon="diagram-tree" text="Sitemap"/>
                    <Button className={Classes.MINIMAL} icon="box" text="Archive"/>
                </NavbarGroup>
            </Navbar>
            <SiteTree />
            <ArchiveTree />
            <TabsContainer />
            <Popup>
                <AddPopup />
            </Popup>
            <DevTools />
        </div>
    </Provider>
));

export default app;
