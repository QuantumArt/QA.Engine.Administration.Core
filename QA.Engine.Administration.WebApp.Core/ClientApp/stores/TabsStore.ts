import { action, observable } from 'mobx';
import { ITreeElement } from './SiteTreeStore';

export enum TabsState {
    NONE,
    COMMON,
    WIDGETS,
}

export interface ITabData {
    id: string | number;
    label: string;
}

export default class TabsStore {
    @observable currentTab: TabsState = TabsState.NONE;
    @observable tabData: ITabData = null;

    @action
    setTab = (id: TabsState) => {
        this.currentTab = id;
    }

    @action
    setTabData = (nodeData: ITreeElement) => {
        if (nodeData.isSelected) {
            this.tabData = this.extractData(nodeData);
            this.currentTab = TabsState.COMMON;
        } else {
            this.currentTab = TabsState.NONE;
            this.tabData = null;
        }
    }

    private extractData = (el: ITreeElement): ITabData => {
        return {
            id: el.id,
            label: el.label,
        };
    }
}
