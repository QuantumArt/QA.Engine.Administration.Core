import * as React from 'react';
import { Dialog, Spinner } from '@blueprintjs/core';
import { inject, observer } from 'mobx-react';
import PopupStore from 'stores/PopupStore';
import OperationState from 'enums/OperationState';

interface Props {
    popupStore?: PopupStore;
}

@inject('popupStore')
@observer
export default class Popup extends React.Component<Props> {

    private handleClose = () => this.props.popupStore.close();

    render() {
        const { popupStore, children } = this.props;
        const isError = popupStore.state === OperationState.ERROR;
        const isLoading = popupStore.state === OperationState.PENDING;

        if (isError) {
            return null;
        }

        return (
            <Dialog
                className="action-dialog"
                isOpen={popupStore.showPopup}
                onClose={this.handleClose}
                title={popupStore.title}
                enforceFocus={false}
            >
                {isLoading ?
                    <Spinner size={30} className="dialog-spinner" /> :
                    children
                }
            </Dialog>
        );
    }
}
