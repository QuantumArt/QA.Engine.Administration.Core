import * as React from 'react';
import { observer } from 'mobx-react';
import { Button, Navbar, NavbarGroup, H5, Intent } from '@blueprintjs/core';

interface Props {

}

@observer
export default class CommonTab extends React.Component<Props> {
    render() {
        return (
            <div className="tab">
                <Navbar className="tab-navbar">
                    <NavbarGroup>
                        <Button minimal icon="refresh" text="Refresh"/>
                        <Button minimal icon="edit" text="Edit" intent={Intent.PRIMARY} />
                        <Button minimal icon="saved" text="Save" intent={Intent.SUCCESS} />
                    </NavbarGroup>
                </Navbar>
                <div className="tab-content">
                    <div className="tab-entity">
                        <H5>ID</H5>
                        <p>77</p>
                    </div>
                    <div className="tab-entity">
                        <H5>TypeName</H5>
                        <p>77</p>
                    </div>
                    <div className="tab-entity">
                        <H5>Title</H5>
                        <p>77</p>
                    </div>
                </div>
            </div>
        );
    }
}
