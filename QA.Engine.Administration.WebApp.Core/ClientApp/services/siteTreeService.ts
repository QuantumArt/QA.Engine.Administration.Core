import HttpService from './httpService';

class SiteTreeService extends HttpService<Models.PageViewModel[]> {
    public async getSiteTree(): Promise<Models.PageViewModel[]> {
        try {
            return await this.get('/api/SiteMap/getAllItems');
        } catch (e) {
            console.log(e);
        }
    }
}

export default new SiteTreeService();
