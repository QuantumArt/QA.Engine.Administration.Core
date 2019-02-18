import * as React from 'react';
import { inject, observer } from 'mobx-react';
import { NavigationState, Pages } from 'stores/NavigationStore';
import { Alignment, Button, Classes, Navbar, NavbarGroup, NavbarHeading } from '@blueprintjs/core';

interface Props {
    navigationStore?: NavigationState;
}

@inject('navigationStore')
@observer
export default class NavigationBar extends React.Component<Props> {
    private handleClick = (pageId: Pages) => () => {
        this.props.navigationStore.changePage(pageId);
    }

    render() {
        return (
            <Navbar fixedToTop>
                <NavbarGroup align={Alignment.LEFT}>
                    <NavbarHeading>Управление Сайтом</NavbarHeading>
                    <Button
                        className={Classes.MINIMAL}
                        icon="diagram-tree"
                        text="Sitemap"
                        onClick={this.handleClick(Pages.SITEMAP)}
                    />
                    <Button
                        className={Classes.MINIMAL}
                        icon="box"
                        text="Archive"
                        onClick={this.handleClick(Pages.ARCHIVE)}
                    />
                </NavbarGroup>
            </Navbar>
        );
    }
}
