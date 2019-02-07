import HttpService from './httpService';

class RemoveService extends HttpService<void> {
    public async remove(itemId: number): Promise<boolean> {
        try {
            const model = <Models.RemoveRequestModel>{
                itemId: itemId,
                isDeleteAllVersions: true,
                isDeleteContentVersions: true,
                contentVersionId: null
            };
            return await this.post('/api/SiteMap/remove', model);
        } catch (e) {
            console.error(e);
        }
    }
}

export default new RemoveService();
