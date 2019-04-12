import * as React from 'react';
import { inject, observer } from 'mobx-react';
import { Alignment, Button, Classes, Intent, Navbar, NavbarGroup, NavbarHeading } from '@blueprintjs/core';
import NavigationStore, { Pages } from 'stores/NavigationStore';
import TreeStore from 'stores/TreeStore';
import TextStore from 'stores/TextStore';
import Texts from 'constants/Texts';
import SiteTreeStore from 'stores/TreeStore/SiteTreeStore';

interface Props {
    navigationStore?: NavigationStore;
    treeStore?: TreeStore;
    textStore?: TextStore;
}

@inject('navigationStore', 'treeStore', 'textStore')
@observer
export default class NavigationBar extends React.Component<Props> {
    componentDidMount() {
        this.changePage(Pages.SITEMAP);
    }

    private handleClick = (pageId: Pages) => async () => {
        const { navigationStore, treeStore } = this.props;
        treeStore.resolveMainTreeStore().clear();
        this.changePage(pageId);
        navigationStore.resetTab();
    }

    private changePage = (pageId: Pages) => {
        const { navigationStore, treeStore } = this.props;
        navigationStore.changePage(pageId);
        treeStore.fetchTree();
    }

    render() {
        const { navigationStore: { currentPage }, textStore, treeStore } = this.props;
        const tree = treeStore.resolveMainTreeStore();
        const isMoveTreeMode = tree instanceof SiteTreeStore ? tree.moveTreeMode : false;
        return (
            <Navbar fixedToTop>
                <NavbarGroup align={Alignment.LEFT}>
                    <NavbarHeading>{textStore.texts[Texts.siteManagement]}</NavbarHeading>
                    <Button
                        className={Classes.MINIMAL}
                        icon="diagram-tree"
                        text={textStore.texts[Texts.siteMapMenu]}
                        onClick={this.handleClick(Pages.SITEMAP)}
                        intent={currentPage === Pages.SITEMAP ? Intent.PRIMARY : Intent.NONE}
                    />
                    <Button
                        className={Classes.MINIMAL}
                        icon="box"
                        text={textStore.texts[Texts.archiveMenu]}
                        onClick={this.handleClick(Pages.ARCHIVE)}
                        intent={currentPage === Pages.ARCHIVE ? Intent.PRIMARY : Intent.NONE}
                        disabled={isMoveTreeMode}
                    />
                </NavbarGroup>
            </Navbar>
        );
    }
}
