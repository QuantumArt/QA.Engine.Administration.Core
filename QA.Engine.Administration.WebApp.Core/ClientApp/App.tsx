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
import ArchiveStore from 'stores/ArchiveStore';
import QpIntegrationStore from 'stores/QpIntegrationStore';
import PopupStore from 'stores/PopupStore';
import TreeStore from 'stores/TreeStore';
import EditArticleStore from 'stores/EditArticleStore';
import Popup from 'components/Popup';
import AddPopup from 'components/Popup/AddPopup';
import AddVersionPopup from 'components/Popup/AddVersionPopup';
import ArchivePopup from 'components/Popup/ArchivePopup';
import DeletePopup from 'components/Popup/DeletePopup';
import RestorePopup from 'components/Popup/RestorePopup';

const treeStore = new TreeStore(SiteTreeStore, ArchiveStore, NavigationStore);

const app = hot(module)(() => (
    <Provider
        treeStore={treeStore}
        qpIntegrationStore={QpIntegrationStore}
        navigationStore={NavigationStore}
        popupStore={PopupStore}
        editArticleStore={EditArticleStore}
    >
        <div className="layout">
            <NavigationBar />
            <SiteTree/>
            <TabsContainer />
            <Popup>
                <AddPopup />
                <AddVersionPopup />
                <ArchivePopup />
                <DeletePopup />
                <RestorePopup />
            </Popup>
            <DevTools/>
        </div>
    </Provider>
));

export default app;
