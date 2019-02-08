import * as React from 'react';
import { observer } from 'mobx-react';
import { Button, Navbar, NavbarGroup, H5, Intent } from '@blueprintjs/core';
import { ITabData } from 'stores/TabsStore';

interface Props {
    data: ITabData;
}

@observer
export default class CommonTab extends React.Component<Props> {
    render() {
        const { data } = this.props;
        if (data !== null) {
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
                            <p>{data.id}</p>
                        </div>
                        <div className="tab-entity">
                            <H5>Title</H5>
                            <p>{data.label}</p>
                        </div>
                    </div>
                </div>
            );
        }

        return null;
    }
}
