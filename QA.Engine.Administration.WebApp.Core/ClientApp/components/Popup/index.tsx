import * as React from 'react';
import { Dialog, Spinner } from '@blueprintjs/core';
import { inject, observer } from 'mobx-react';
import { PopupState } from 'stores/PopupStore';
import OperationState from 'enums/OperationState';

interface Props {
    popupStore?: PopupState;
}

@inject('popupStore')
@observer
export default class Popup extends React.Component<Props> {
    private handleClose = () => this.props.popupStore.close();

    render() {
        const { popupStore, children } = this.props;
        const isLoading = popupStore.state === OperationState.NONE || popupStore.state === OperationState.PENDING;

        return (
            <Dialog
                className="action-dialog"
                isOpen={popupStore.showPopup}
                onClose={this.handleClose}
                title={popupStore.title}
            >
                {isLoading ? <Spinner size={30} className="dialog-spinner" /> : children}
            </Dialog>
        );
    }
}
