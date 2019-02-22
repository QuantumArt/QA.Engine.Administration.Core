import * as React from 'react';
import { inject, observer } from 'mobx-react';
import { Alignment, Button, Classes, Intent, Navbar, NavbarGroup, NavbarHeading } from '@blueprintjs/core';
import NavigationStore, { Pages } from 'stores/NavigationStore';
import TreeStore from 'stores/TreeStore';

interface Props {
    navigationStore?: NavigationStore;
    treeStore?: TreeStore;
}

@inject('navigationStore', 'treeStore')
@observer
export default class NavigationBar extends React.Component<Props> {
    componentDidMount() {
        this.changePage(Pages.SITEMAP);
    }

    private handleClick = (pageId: Pages) => async () => {
        const { navigationStore } = this.props;
        this.changePage(pageId);
        navigationStore.resetTab();
    }

    private changePage = (pageId: Pages) => {
        const { navigationStore, treeStore } = this.props;
        navigationStore.changePage(pageId);
        treeStore.resolveTreeStore().fetchTree();
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
