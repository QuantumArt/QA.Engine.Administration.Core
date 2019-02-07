import HttpService from './httpService';

class ArchiveService extends HttpService<Models.ArchiveViewModel> {
    public async getArchive(): Promise<Models.ArchiveViewModel> {
        try {
            return await this.get('/api/SiteMap/getAllArchiveItems');
        } catch (e) {
            console.log(e);
        }
    }
}

export default new ArchiveService();
