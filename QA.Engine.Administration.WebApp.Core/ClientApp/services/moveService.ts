import HttpService from './httpService';

class MoveService extends HttpService<void> {
    public async move(itemId: number, newParentId: number): Promise<boolean> {
        try {
            const model = <Models.MoveRequestModel>{
                itemId: itemId,
                newParentId: newParentId
            };
            return await this.post('/api/SiteMap/move', model);
        } catch (e) {
            console.error(e);
        }
    }
}

export default new MoveService();
