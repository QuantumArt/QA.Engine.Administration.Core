import * as React from 'react';
import { Provider } from 'mobx-react';
import DevTools from 'mobx-react-devtools'; // tslint:disable-line
import { hot } from 'react-hot-loader';
import '@blueprintjs/core/lib/css/blueprint.css';
import '@blueprintjs/icons/lib/css/blueprint-icons.css';
import 'normalize.css/normalize.css';

import 'assets/style.css';
import SiteTree from 'components/SiteTree';
import TabsContainer from 'components/TabsContainer';
import NavigationBar from 'components/NavigationBar';
import NavigationStore from 'stores/NavigationStore';
import SiteTreeStore from 'stores/SiteTreeStore';
import TabsStore from 'stores/TabsStore';
import ArchiveStore from 'stores/ArchiveStore';
import QpIntegrationStore from 'stores/QpIntegrationStore';
import ExtensionFieldsStore from 'stores/ExtensionFieldsStore';
import Popup from 'components/Popup/popup';
import PopupStore from 'stores/PopupStore';
import AddPopup from 'components/Popup/add';
import AddVersionPopup from 'components/Popup/addVersion';
import ArchivePopup from 'components/Popup/archive';

const app = hot(module)(() => (
    <Provider
        siteTreeStore={SiteTreeStore}
        archiveStore={ArchiveStore}
        tabsStore={TabsStore}
        qpIntegrationStore={QpIntegrationStore}
        extensionFieldsStore={ExtensionFieldsStore}
        navigationStore={NavigationStore}
        popupStore={PopupStore}
    >
        <div className="layout">
            <NavigationBar />
            <SiteTree/>
            <TabsContainer />
            <Popup>
                <AddPopup />
                <AddVersionPopup />
                <ArchivePopup />
            </Popup>
            <DevTools/>
        </div>
    </Provider>
));

export default app;
