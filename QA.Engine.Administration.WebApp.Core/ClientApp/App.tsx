import * as React from 'react';
import { Provider } from 'mobx-react';
import DevTools from 'mobx-react-devtools'; // tslint:disable-line
import { hot } from 'react-hot-loader';
import '@blueprintjs/core/lib/css/blueprint.css';
import '@blueprintjs/icons/lib/css/blueprint-icons.css';
import 'normalize.css/normalize.css';

import 'assets/style.css';
import TreeStructure from 'components/TreeStructure';
import TabsContainer from 'components/TabsContainer';
import NavigationBar from 'components/NavigationBar';
import NavigationStore from 'stores/NavigationStore';
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
import ErrorToast from 'components/ErrorToast';
import TextStore from 'stores/TextStore';
import ReorderPopup from 'components/Popup/ReorderPopup';
import MovePopup from 'components/Popup/MovePopup';

const app = hot(module)(() => {
    const navigationStoreInstance = new NavigationStore();
    const treeStoreInstance = new TreeStore(navigationStoreInstance);
    const textStore = new TextStore();

    return (
        <Provider
            treeStore={treeStoreInstance}
            qpIntegrationStore={new QpIntegrationStore(treeStoreInstance)}
            navigationStore={navigationStoreInstance}
            popupStore={new PopupStore()}
            editArticleStore={new EditArticleStore()}
            textStore={textStore}
        >
        <div className="layout">
            <NavigationBar/>
            <TreeStructure
                type="site"
                treeStore={treeStoreInstance}
            />
            <TabsContainer/>
            <Popup>
                <AddPopup/>
                <AddVersionPopup/>
                <ArchivePopup/>
                <DeletePopup/>
                <RestorePopup/>
                <ReorderPopup/>
                <MovePopup/>
            </Popup>
            <ErrorToast/>
            <DevTools/>
        </div>
    </Provider>
    );
});

export default app;
