import * as React from 'react';
import { inject, observer } from 'mobx-react';
import { Alignment, Button, Classes, Intent, Navbar, NavbarGroup, NavbarHeading } from '@blueprintjs/core';
import { NavigationState, Pages } from 'stores/NavigationStore';
import { SiteTreeState } from 'stores/SiteTreeStore';
import { ArchiveState } from 'stores/ArchiveStore';
import { TabsState } from 'stores/TabsStore';

interface Props {
    navigationStore?: NavigationState;
    siteTreeStore?: SiteTreeState;
    archiveStore?: ArchiveState;
    tabsStore?: TabsState;
}

@inject('navigationStore', 'siteTreeStore', 'archiveStore', 'tabsStore')
@observer
export default class NavigationBar extends React.Component<Props> {
    componentDidMount() {
        this.changePage(Pages.SITEMAP);
    }

    private handleClick = (pageId: Pages) => async () => {
        const { tabsStore } = this.props;
        await this.changePage(pageId);
        tabsStore.resetTab();
    }

    private changePage = async (pageId: Pages) => {
        const { siteTreeStore , archiveStore, navigationStore } = this.props;
        navigationStore.changePage(pageId);
        if (pageId === Pages.SITEMAP) {
            await siteTreeStore.fetchTree();
        } else if (pageId === Pages.ARCHIVE) {
            await archiveStore.fetchTree();
        }
    }

    render() {
        const { navigationStore: { currentPage } } = this.props;
        return (
            <Navbar fixedToTop>
                <NavbarGroup align={Alignment.LEFT}>
                    <NavbarHeading>Управление Сайтом</NavbarHeading>
                    <Button
                        className={Classes.MINIMAL}
                        icon="diagram-tree"
                        text="Карта Сайта"
                        onClick={this.handleClick(Pages.SITEMAP)}
                        intent={currentPage === Pages.SITEMAP ? Intent.PRIMARY : Intent.NONE}
                    />
                    <Button
                        className={Classes.MINIMAL}
                        icon="box"
                        text="Архив"
                        onClick={this.handleClick(Pages.ARCHIVE)}
                        intent={currentPage === Pages.ARCHIVE ? Intent.PRIMARY : Intent.NONE}
                    />
                </NavbarGroup>
            </Navbar>
        );
    }
}
