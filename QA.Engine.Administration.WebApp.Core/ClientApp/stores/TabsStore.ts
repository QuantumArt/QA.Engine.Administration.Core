import { action, observable } from 'mobx';
import { ITreeElement } from 'stores/BaseTreeStore';

export enum TabTypes {
    NONE,
    COMMON,
    WIDGETS,
}

export interface ITabData {
    id: string | number;
    label: string;
}

export class TabsState {
    @observable currentTab: TabTypes = TabTypes.NONE;
    @observable tabData: ITabData = null;

    @action
    setTab = (id: TabTypes) => {
        this.currentTab = id;
    }

    @action
    setTabData = (nodeData: ITreeElement) => {
        if (nodeData.isSelected) {
            this.tabData = this.extractData(nodeData);
            this.currentTab = TabTypes.COMMON;
        } else {
            this.currentTab = TabTypes.NONE;
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

const tabsStore = new TabsState();
export default tabsStore;
