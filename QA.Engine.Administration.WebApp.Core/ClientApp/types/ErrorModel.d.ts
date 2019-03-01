declare interface IErrorModel<T> {
    type: T;
    message: string;
    data?: any;
    id: string;
}
