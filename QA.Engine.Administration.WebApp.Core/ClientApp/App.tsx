import * as React from 'react';
import { Provider } from 'mobx-react';
import DevTools from 'mobx-react-devtools'
import { hot } from 'react-hot-loader';
import { Alignment, Button, Classes, Navbar, NavbarGroup, NavbarHeading } from '@blueprintjs/core';
import '@blueprintjs/core/lib/css/blueprint.css';
import '@blueprintjs/icons/lib/css/blueprint-icons.css';
import 'normalize.css/normalize.css';
import 'assets/style.css';

import SiteTree from 'components/SiteTree';
import configureStores from 'stores/configureStores';

const stores = configureStores();

const app = hot(module)(() => (
    <Provider siteTreeStore={stores.siteTreeStore} archiveStore={stores.archiveStrore}>
        <div className="layout">
            <Navbar fixedToTop>
                <NavbarGroup align={Alignment.LEFT}>
                    <NavbarHeading>Manage Site</NavbarHeading>
                    <Button className={Classes.MINIMAL} icon="diagram-tree" text="Sitemap"/>
                    <Button className={Classes.MINIMAL} icon="box" text="Archive"/>
                </NavbarGroup>
            </Navbar>
            <SiteTree/>
            <DevTools />
        </div>
    </Provider>
));

export default app;
