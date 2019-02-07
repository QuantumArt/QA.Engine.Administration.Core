import HttpService from './httpService';

export interface IApiResultWidgetTreeModel {
    data?: IWidgetTreeModel[];
    isSuccess?: boolean;
    error?: string;
}

export interface ISiteTreeModel {
    id?: number; // int32
    isArchive?: boolean;
    parentId?: number; // int32
    alias?: string;
    title?: string;
    zoneName?: string;
    extensionId?: number; // int32
    indexOrder?: number; // int32
    isVisible?: boolean;
    isPage?: boolean;
    versionOfId?: number; // int32
    published?: boolean;
    isInSiteMap?: boolean;
    discriminatorId?: number; // int32
    widgets?: IWidgetTreeModel[];
    children?: ISiteTreeModel[];
    contentVersions?: ISiteTreeModel[];
    discriminator?: IDiscriminatorModel;
    readonly hasWidgets?: boolean;
    readonly hasChildren?: boolean;
    readonly hasContentVersion?: boolean;
}

export interface IWidgetTreeModel {
    id?: number; // int32
    isArchive?: boolean;
    parentId?: number; // int32
    alias?: string;
    title?: string;
    zoneName?: string;
    extensionId?: number; // int32
    indexOrder?: number; // int32
    isVisible?: boolean;
    isPage?: boolean;
    versionOfId?: number; // int32
    published?: boolean;
    discriminatorId?: number; // int32
    children?: IWidgetTreeModel[];
    discriminator?: IDiscriminatorModel;
    readonly hasChildren?: boolean;
}

export interface IDiscriminatorModel {
    id?: number; // int32
    isArchive?: boolean;
    discriminator?: string;
    typeName?: string;
    isPage?: boolean;
    title?: string;
    description?: string;
    iconUrl?: string;
    preferredContentId?: number; // int32
}

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
