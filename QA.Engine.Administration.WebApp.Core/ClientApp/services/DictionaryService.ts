﻿/****************************************************************************
  Generated by TypeWriter - don't make any changes in this file
****************************************************************************/

/** Api справочников */
class DictionaryService {

    /** Возвращает типы контента */
    public async getDescriminators(): Promise<ApiResult<DiscriminatorViewModel[]>> {

        const path = '/api/Dictionary/getDefinitions';
        const headers = new Headers();
        headers.append('Qp-Site-Params', JSON.stringify(this.getHeaderData()));
        const init = {
            headers,
            method: 'get',
        };

        console.debug(`%cstart api request get '${path}'`, 'color: green;');
        const response = await fetch(path, init);

        const result = await <Promise<ApiResult<DiscriminatorViewModel[]>>>response.json();
        console.log(`%cresult api get '${path}'`, 'color: blue;', result);

        return result;
    }

    /** Возвращает регионы списком */
    public async getFlatRegions(): Promise<ApiResult<RegionViewModel[]>> {

        const path = '/api/Dictionary/getFlatRegions';
        const headers = new Headers();
        headers.append('Qp-Site-Params', JSON.stringify(this.getHeaderData()));
        const init = {
            headers,
            method: 'get',
        };

        console.debug(`%cstart api request get '${path}'`, 'color: green;');
        const response = await fetch(path, init);

        const result = await <Promise<ApiResult<RegionViewModel[]>>>response.json();
        console.log(`%cresult api get '${path}'`, 'color: blue;', result);

        return result;
    }

    /** Возвращает дерево регионов */
    public async getRegionTree(): Promise<ApiResult<RegionViewModel[]>> {

        const path = '/api/Dictionary/getRegionTree';
        const headers = new Headers();
        headers.append('Qp-Site-Params', JSON.stringify(this.getHeaderData()));
        const init = {
            headers,
            method: 'get',
        };

        console.debug(`%cstart api request get '${path}'`, 'color: green;');
        const response = await fetch(path, init);

        const result = await <Promise<ApiResult<RegionViewModel[]>>>response.json();
        console.log(`%cresult api get '${path}'`, 'color: blue;', result);

        return result;
    }

    /** Возвращает контент qp с полями */
    public async getQpContent(contentName: string): Promise<ApiResult<QpContentViewModel>> {

        let urlparams = '';
        urlparams += Array.isArray(contentName) && contentName.length === 0 ? '' : `&contentName=${contentName} `;
        urlparams = urlparams.length > 0 ? `?${urlparams.slice(1)}` : '';
        const path = `/api/Dictionary/getQpContent${urlparams}`;
        const headers = new Headers();
        headers.append('Qp-Site-Params', JSON.stringify(this.getHeaderData()));
        const init = {
            headers,
            method: 'get',
        };

        console.debug(`%cstart api request get '${path}'`, 'color: green;');
        const response = await fetch(path, init);

        const result = await <Promise<ApiResult<QpContentViewModel>>>response.json();
        console.log(`%cresult api get '${path}'`, 'color: blue;', result);

        return result;
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

export default new DictionaryService();
