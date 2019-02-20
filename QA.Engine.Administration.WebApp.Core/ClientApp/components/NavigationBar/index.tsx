import * as React from 'react';
import { inject, observer } from 'mobx-react';
import { Alignment, Button, Classes, Intent, Navbar, NavbarGroup, NavbarHeading } from '@blueprintjs/core';
import { NavigationState, Pages } from 'stores/NavigationStore';

interface Props {
    navigationStore?: NavigationState;
}

@inject('navigationStore')
@observer
export default class NavigationBar extends React.Component<Props> {
    componentDidMount() {
        this.changePage(Pages.SITEMAP);
    }

    private handleClick = (pageId: Pages) => async () => {
        const { navigationStore } = this.props;
        await this.changePage(pageId);
        navigationStore.resetTab();
    }

    private changePage = async (pageId: Pages) => {
        const { navigationStore } = this.props;
        const treeStore = navigationStore.resolveTreeStore();
        navigationStore.changePage(pageId);
        treeStore.fetchTree();
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
