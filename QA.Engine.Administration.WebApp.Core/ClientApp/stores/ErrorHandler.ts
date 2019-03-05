import v4 from 'uuid/v4';
import { action, observable } from 'mobx';
import ErrorsTypes from 'constants/ErrorsTypes';
import OperationState from 'enums/OperationState';

export interface IErrorModel {
    type: ErrorsTypes;
    message: string;
    data?: any;
    id: string;
}

export default abstract class ErrorHandler {
    @observable abstract state: OperationState;

    @observable errors: IErrorModel[] = [];

    @action addError = (type: ErrorsTypes, data: any, message: string) => {
        this.errors.push({ type, data, message, id: v4() });
    }

    @action
    public removeError = (i: number) => {
        if (this.state) {
            this.state = OperationState.NONE;
        }
        this.errors.splice(i, 1);
    }
}
