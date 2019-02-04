import * as qs from 'qs';

class SiteTreeApi {
    public async getSiteTree() {
        try {
            const params = qs.stringify({
                backend_sid: 'c3386b2f-e098-4dfb-a794-e774cba9fcfc',
                customerCode: 'qa_demosite',
                site_id: 52,
            });
            const res = await fetch(`/api/SiteMap/getAllItems?${params}`);
            const jsonData =  await res.json();

            return jsonData.data;
        } catch (e) {
            console.log(e);
        }
    }

    // private mapData(data) {

    // }
}

const siteTreeService = new SiteTreeApi();
export default siteTreeService;
