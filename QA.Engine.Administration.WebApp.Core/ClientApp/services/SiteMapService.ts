﻿/****************************************************************************
  Generated by TypeWriter - don't make any changes in this file
****************************************************************************/

/** Api получения карты сайта */
class SiteMapService {

    /** Возвращает полное дерево карты сайта */
    public async getSiteMapTree(regionIds: number[] = null): Promise<ApiResult<PageViewModel[]>> {

        let urlparams = '';
        urlparams += Array.isArray(regionIds) && regionIds.length === 0 ? '' : (regionIds == null ? '' : `&regionIds=${regionIds} `);
        urlparams = urlparams.length > 0 ? `?${urlparams.slice(1)}` : '';
        const path = `api/SiteMap/getSiteMapTree${urlparams}`;
        const headers = new Headers();
        headers.append('Qp-Site-Params', JSON.stringify(this.getHeaderData()));
        const init = {
            headers,
            method: 'get',
        };

        console.debug(`%cstart api request get '${path}'`, 'color: green;');
        const response = await fetch(path, init);

        const result = await <Promise<ApiResult<PageViewModel[]>>>response.json();
        console.log(`%cresult api get '${path}'`, 'color: blue;', result);

        return result;
    }

    /** Возвращает полную структуру архива */
    public async getArchiveTree(): Promise<ApiResult<ArchiveViewModel>> {

        const path = '/api/SiteMap/getArchiveTree';
        const headers = new Headers();
        headers.append('Qp-Site-Params', JSON.stringify(this.getHeaderData()));
        const init = {
            headers,
            method: 'get',
        };

        console.debug(`%cstart api request get '${path}'`, 'color: green;');
        const response = await fetch(path, init);

        const result = await <Promise<ApiResult<ArchiveViewModel>>>response.json();
        console.log(`%cresult api get '${path}'`, 'color: blue;', result);

        return result;
    }

    /** Возвращает дочерние страницы родительского элемента (страницы) */
    public async getPageTree(isArchive: boolean, parentId?: number, regionIds: number[] = null): Promise<ApiResult<PageViewModel[]>> {

        let urlparams = '';
        urlparams += Array.isArray(isArchive) && isArchive.length === 0 ? '' : `&isArchive=${isArchive} `;
        urlparams += Array.isArray(parentId) && parentId.length === 0 ? '' : (parentId == null ? '' : `&regionIds=${parentId} `);
        urlparams += Array.isArray(regionIds) && regionIds.length === 0 ? '' : (regionIds == null ? '' : `&regionIds=${regionIds} `);
        urlparams = urlparams.length > 0 ? `?${urlparams.slice(1)}` : '';
        const path = `api/SiteMap/getPageTree${urlparams}`;
        const headers = new Headers();
        headers.append('Qp-Site-Params', JSON.stringify(this.getHeaderData()));
        const init = {
            headers,
            method: 'get',
        };

        console.debug(`%cstart api request get '${path}'`, 'color: green;');
        const response = await fetch(path, init);

        const result = await <Promise<ApiResult<PageViewModel[]>>>response.json();
        console.log(`%cresult api get '${path}'`, 'color: blue;', result);

        return result;
    }

    /** Возвращает дочерние виджеты у родительского элемента (страницы или виджета) */
    public async getWidgetTree(isArchive: boolean, parentId: number, regionIds: number[] = null): Promise<ApiResult<WidgetViewModel[]>> {

        let urlparams = '';
        urlparams += Array.isArray(isArchive) && isArchive.length === 0 ? '' : `&isArchive=${isArchive} `;
        urlparams += Array.isArray(parentId) && parentId.length === 0 ? '' : `&parentId=${parentId} `;
        urlparams += Array.isArray(regionIds) && regionIds.length === 0 ? '' : (regionIds == null ? '' : `&regionIds=${regionIds} `);
        urlparams = urlparams.length > 0 ? `?${urlparams.slice(1)}` : '';
        const path = `api/SiteMap/getWidgetTree${urlparams}`;
        const headers = new Headers();
        headers.append('Qp-Site-Params', JSON.stringify(this.getHeaderData()));
        const init = {
            headers,
            method: 'get',
        };

        console.debug(`%cstart api request get '${path}'`, 'color: green;');
        const response = await fetch(path, init);

        const result = await <Promise<ApiResult<WidgetViewModel[]>>>response.json();
        console.log(`%cresult api get '${path}'`, 'color: blue;', result);

        return result;
    }

    /** Опубликовать страницу */
    public async publish(itemIds: number[]): Promise<any> {

        const path = '/api/SiteMap/publish';
        const headers = new Headers();
        headers.append('Qp-Site-Params', JSON.stringify(this.getHeaderData()));
        headers.append('Content-Type', 'application/json');
        const init = {
            headers,
            method: 'post',
            body: JSON.stringify(itemIds),
        };

        console.debug(`%cstart api request post '${path}'`, 'color: green;');
        const response = await fetch(path, init);

        const result = await <Promise<any>>response.json();
        console.log(`%cresult api post '${path}'`, 'color: blue;', result);

        return result;
    }

    /** Изменить порядок отображения страниц */
    public async reorder(model: ReorderModel): Promise<any> {

        const path = '/api/SiteMap/reorder';
        const headers = new Headers();
        headers.append('Qp-Site-Params', JSON.stringify(this.getHeaderData()));
        headers.append('Content-Type', 'application/json');
        const init = {
            headers,
            method: 'post',
            body: JSON.stringify(model),
        };

        console.debug(`%cstart api request post '${path}'`, 'color: green;');
        const response = await fetch(path, init);

        const result = await <Promise<any>>response.json();
        console.log(`%cresult api post '${path}'`, 'color: blue;', result);

        return result;
    }

    /** Переместить страницу к новому родительскому элементу */
    public async move(model: MoveModel): Promise<any> {

        const path = '/api/SiteMap/move';
        const headers = new Headers();
        headers.append('Qp-Site-Params', JSON.stringify(this.getHeaderData()));
        headers.append('Content-Type', 'application/json');
        const init = {
            headers,
            method: 'post',
            body: JSON.stringify(model),
        };

        console.debug(`%cstart api request post '${path}'`, 'color: green;');
        const response = await fetch(path, init);

        const result = await <Promise<any>>response.json();
        console.log(`%cresult api post '${path}'`, 'color: blue;', result);

        return result;
    }

    /** Редактировать */
    public async edit(model: EditModel): Promise<any> {

        const path = '/api/SiteMap/edit';
        const headers = new Headers();
        headers.append('Qp-Site-Params', JSON.stringify(this.getHeaderData()));
        headers.append('Content-Type', 'application/json');
        const init = {
            headers,
            method: 'post',
            body: JSON.stringify(model),
        };

        console.debug(`%cstart api request post '${path}'`, 'color: green;');
        const response = await fetch(path, init);

        const result = await <Promise<any>>response.json();
        console.log(`%cresult api post '${path}'`, 'color: blue;', result);

        return result;
    }

    /** Удаление элементов в архив */
    public async remove(model: RemoveModel): Promise<any> {

        const path = '/api/SiteMap/remove';
        const headers = new Headers();
        headers.append('Qp-Site-Params', JSON.stringify(this.getHeaderData()));
        headers.append('Content-Type', 'application/json');
        const init = {
            headers,
            method: 'post',
            body: JSON.stringify(model),
        };

        console.debug(`%cstart api request post '${path}'`, 'color: green;');
        const response = await fetch(path, init);

        const result = await <Promise<any>>response.json();
        console.log(`%cresult api post '${path}'`, 'color: blue;', result);

        return result;
    }

    /** Восстановление элементов */
    public async restore(model: RestoreModel): Promise<any> {

        const path = '/api/SiteMap/restore';
        const headers = new Headers();
        headers.append('Qp-Site-Params', JSON.stringify(this.getHeaderData()));
        headers.append('Content-Type', 'application/json');
        const init = {
            headers,
            method: 'post',
            body: JSON.stringify(model),
        };

        console.debug(`%cstart api request post '${path}'`, 'color: green;');
        const response = await fetch(path, init);

        const result = await <Promise<any>>response.json();
        console.log(`%cresult api post '${path}'`, 'color: blue;', result);

        return result;
    }

    /** Удаление элементов */
    public async delete(model: DeleteModel): Promise<any> {

        const path = '/api/SiteMap/delete';
        const headers = new Headers();
        headers.append('Qp-Site-Params', JSON.stringify(this.getHeaderData()));
        headers.append('Content-Type', 'application/json');
        const init = {
            headers,
            method: 'post',
            body: JSON.stringify(model),
        };

        console.debug(`%cstart api request post '${path}'`, 'color: green;');
        const response = await fetch(path, init);

        const result = await <Promise<any>>response.json();
        console.log(`%cresult api post '${path}'`, 'color: blue;', result);

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

export default new SiteMapService();