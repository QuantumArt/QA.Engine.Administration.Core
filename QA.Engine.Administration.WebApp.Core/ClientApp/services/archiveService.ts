import * as qs from 'qs';

class ArchiveService {
    public async getSiteTree(): Promise<Models.ArchiveViewModel> {
        try {
            const params = qs.stringify({
                backend_sid: 'c3386b2f-e098-4dfb-a794-e774cba9fcfc',
                customerCode: 'qa_demosite',
                site_id: 52,
            });
            const res = await fetch(`/api/SiteMap/getAllArchiveItems?${params}`);

            return await res.json();
        } catch (e) {
            console.log(e);
        }
    }
}

export default new ArchiveService();
