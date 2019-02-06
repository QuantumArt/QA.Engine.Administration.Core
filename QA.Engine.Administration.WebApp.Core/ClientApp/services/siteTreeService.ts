import * as qs from 'qs';

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

class SiteTreeApi {
    public async getSiteTree() {
        try {
            const params = qs.stringify({
                backend_sid: 'c3386b2f-e098-4dfb-a794-e774cba9fcfc',
                customerCode: 'qa_demosite',
                site_id: 52,
            });
            const res = await fetch(`/api/SiteMap/getAllItems?${params}`);

            return await res.json();
        } catch (e) {
            console.log(e);
        }
    }

    // private mapData(data) {

    // }
}

const siteTreeService = new SiteTreeApi();
export default siteTreeService;
