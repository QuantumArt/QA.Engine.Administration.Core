import HttpService from './HttpService';

class ArchiveService extends HttpService<ArchiveViewModel> {
    public async getArchive(): Promise<ArchiveViewModel> {
        try {
            return await this.get('/api/SiteMap/getAllArchiveItems');
        } catch (e) {
            console.log(e);
        }
    }
}

export default new ArchiveService();
