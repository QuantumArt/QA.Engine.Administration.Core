import { observable, action, runInAction, computed } from 'mobx';
import siteTreeService from 'services/siteTreeService';
import { ITreeNode } from '@blueprintjs/core';

enum TreeState {
    NONE,
    PENDING,
    ERROR,
}

export default class UIStore {
    @observable siteTreeState: TreeState = TreeState.NONE;
    @observable siteTree = {};
    @action async fetchSiteTree() {
        try {
            this.siteTreeState = TreeState.PENDING;
            const tree = await siteTreeService.getSiteTree();
            runInAction(() => {
                this.siteTree = tree;
            });
        } catch (e) {
            console.log(e);
            this.siteTreeState = TreeState.ERROR;
        }
    }
    // @computed get tree() {
    //     // const uiTree: ITreeNode[] =
    // }
}
