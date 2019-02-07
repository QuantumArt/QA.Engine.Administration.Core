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
            method: 'GET',
            headers: headers
        };

        const res = await fetch(path, init);
        console.log('get response', res);

        var result = await <Promise<ApiResult<T>>>res.json();
        console.log('get result', result);

        if (!result.isSuccess) {
            console.error(`get '${path}' error`, result.error);
        }

        return result.data;
    }

    protected async post(path: string, body: any): Promise<boolean> {

        const headers = new Headers();
        headers.append('Qp-Site-Params', JSON.stringify(this.getHeaderData()));
        const init = {
            method: 'POST',
            headers: headers,
            body
        };

        const res = await fetch(path, init);
        console.log('post response', res);

        var result = await <Promise<ApiResult<null>>>res.json();
        console.log('post result', result);

        if (!result.isSuccess) {
            console.error(`post '${path}' error`, result.error);
        }

        return result.isSuccess;
    }

    private getHeaderData(): any {
        const getQueryVariable = (variable: string) => {
            const result = window.location.search.substring(1).split('&')
                .map(x => ({ name: x.split('=')[0], value: x.split('=')[1] }))
                .filter(x => x.name == variable)[0];
            return result == null ? null : result.value;
        };

        return {
            BackendSid: getQueryVariable('backend_sid'),
            CustomerCode: getQueryVariable('customerCode'),
            HostId: getQueryVariable('hostUID'),
            SiteId: getQueryVariable('site_id')
        };
    }

}

export default HttpService;
