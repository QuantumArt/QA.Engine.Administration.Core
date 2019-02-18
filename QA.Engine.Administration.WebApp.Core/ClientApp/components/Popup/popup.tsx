import * as React from 'react';
import { Dialog } from '@blueprintjs/core';
import { inject, observer } from 'mobx-react';
import { PopupState } from 'stores/PopupStore';

interface Props {
    popupStore?: PopupState;
}

@inject('popupStore')
@observer
export default class Popup extends React.Component<Props> {

    private closeClick = () => {
        const { popupStore } = this.props;
        popupStore.close();
    }

    render() {
        const { popupStore, children } = this.props;
        return (
            <Dialog
                isOpen={popupStore.showPopup}
                onClose={this.closeClick}
                title={popupStore.title}
            >
                {children}
            </Dialog>
        );
    }
}
