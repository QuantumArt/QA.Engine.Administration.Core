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
import Popup from 'components/Popup/popup';
import PopupStore from 'stores/PopupStore';
import AddPopup from 'components/Popup/add';
import AddVersionPopup from 'components/Popup/addVersion';
import ArchivePopup from 'components/Popup/archive';
import DeletePopup from 'components/Popup/delete';
import RestorePopup from 'components/Popup/restore';
import EditArticleStore from 'stores/EditArticleStore';
import TreeStore from 'stores/TreeStore';

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
