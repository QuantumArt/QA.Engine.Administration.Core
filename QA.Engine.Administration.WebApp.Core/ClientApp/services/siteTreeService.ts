import HttpService from './HttpService';

class SiteTreeService extends HttpService<PageViewModel[]> {
    public async getSiteTree(): Promise<PageViewModel[]> {
        try {
            return await this.get('/api/SiteMap/getAllItems');
        } catch (e) {
            console.log(e);
        }
    }
}

export default new SiteTreeService();
