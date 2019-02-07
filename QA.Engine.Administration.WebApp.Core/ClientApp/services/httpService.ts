class ApiResult<T> {
    public isSuccess: boolean = false;
    public error: string = '';
    public data: T = null;
}

abstract class HttpService<T> {

    protected async get(path: string): Promise<T> {

        const headers = new Headers();
        headers.append('Qp-Site-Params', JSON.stringify(this.getHeaderData()));
        const init = {
            headers,
            method: 'GET',
        };

        const res = await fetch(path, init);

        const result = await <Promise<ApiResult<T>>>res.json();
        console.log(`api get '${path}' result`, result);

        if (!result.isSuccess) {
            console.error(`api get '${path}' error`, result.error);
            throw `Api GET '${path}' error. Error: ${result.error}`;
        }

        return result.data;
    }

    protected async post(path: string, model: any): Promise<boolean> {

        const headers = new Headers();
        headers.append('Qp-Site-Params', JSON.stringify(this.getHeaderData()));
        headers.append('Content-Type', 'application/json');
        const init = {
            headers,
            method: 'POST',
            body: JSON.stringify(model),
        };

        const res = await fetch(path, init);

        const result = await <Promise<ApiResult<null>>>res.json();
        console.log(`api post '${path}' result`, result);

        if (!result.isSuccess) {
            console.error(`api get '${path}' error`, result.error);
            throw `Api GET '${path}' error. Error: ${result.error}`;
        }

        return result.isSuccess;
    }

    private getHeaderData(): any {
        const getQueryVariable = (variable: string) => {
            const result = window.location.search.substring(1).split('&')
                .map(x => ({ name: x.split('=')[0], value: x.split('=')[1] }))
                .filter(x => x.name === variable)[0];
            return result == null ? null : result.value;
        };

        return {
            BackendSid: getQueryVariable('backend_sid'),
            CustomerCode: getQueryVariable('customerCode'),
            HostId: getQueryVariable('hostUID'),
            SiteId: getQueryVariable('site_id'),
        };
    }

}

export default HttpService;
