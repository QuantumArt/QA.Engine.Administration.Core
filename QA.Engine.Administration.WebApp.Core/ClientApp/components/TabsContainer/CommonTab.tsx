import * as React from 'react';
import { observer, inject } from 'mobx-react';
import { Button, Navbar, NavbarGroup, H5, Intent, Spinner, InputGroup } from '@blueprintjs/core';
import { ITabData } from 'stores/TabsStore';
import { SiteTreeState } from 'stores/SiteTreeStore';
import ExtantionCard from './ExtensionCard';
import { ExtensionFieldsState } from 'stores/ExtensionFieldsStore';
import OperationState from 'enums/OperationState';

interface Props {
    data: ITabData;
    siteTreeStore?: SiteTreeState;
    extensionFieldsStore?: ExtensionFieldsState;
}

interface State {
    isEditMode: boolean;
    title: string;
}

@inject('siteTreeStore', 'extensionFieldsStore')
@observer
export default class CommonTab extends React.Component<Props, State> {

    constructor(props: Props, state: State) {
        super(props);
        this.state = { isEditMode: false, title: '' };
    }

    componentWillReceiveProps(nextProps: Props) {
        const { siteTreeStore, data } = nextProps;
        const node: PageModel | ArchiveModel = data == null ? null : siteTreeStore.getNodeById(+data.id);
        this.setState({ title: node == null ? null : node.title, isEditMode: false });
    }

    private refreshClick = () => {
        const { siteTreeStore, data } = this.props;
        siteTreeStore.updateSubTree(+data.id).then(() => {
            const node: PageModel | ArchiveModel = data == null ? null : siteTreeStore.getNodeById(+data.id);
            this.setState({ title: node.title, isEditMode: false });
        });
    }

    private editClick = () => {
        this.setState({ isEditMode: true });
    }

    private saveClick = () => {
        const { siteTreeStore, extensionFieldsStore, data } = this.props;
        const { title } = this.state;
        const node: PageModel | ArchiveModel = data == null ? null : siteTreeStore.getNodeById(+data.id);
        const model: EditModel = {
            title,
            itemId: +data.id,
            extensionId: node.extensionId,
            fields: extensionFieldsStore.changedFields,
        };
        siteTreeStore.edit(model).then(() => {
            this.setState({ isEditMode: false });
        });
    }

    private changeTitle = (e: React.ChangeEvent<HTMLInputElement>) => {
        this.setState({ title: e.target.value });
    }

    render() {
        const { siteTreeStore, data } = this.props;
        const { isEditMode, title } = this.state;
        const node: PageModel | ArchiveModel = data == null ? null : siteTreeStore.getNodeById(+data.id);
        if (siteTreeStore.treeState === OperationState.NONE || siteTreeStore.treeState === OperationState.PENDING) {
            return (<Spinner size={60} />);
        }
        if (data !== null) {
            return (
                <div className="tab">
                    <Navbar className="tab-navbar">
                        <NavbarGroup>
                            <Button minimal icon="refresh" text="Refresh" onClick={this.refreshClick}/>
                            <Button minimal icon="edit" text="Edit" intent={Intent.PRIMARY} onClick={this.editClick} />
                            <Button minimal icon="saved" text="Save" intent={Intent.SUCCESS} onClick={this.saveClick} />
                        </NavbarGroup>
                    </Navbar>
                    <div className="tab-content">
                        <div className="tab-entity">
                            <H5>ID</H5>
                            <p>{data.id}</p>
                        </div>
                        <div className="tab-entity">
                            <H5>Title</H5>
                            {isEditMode ? (<InputGroup value={title} onChange={this.changeTitle} />) : (<p>{title}</p>)}
                        </div>
                        <div className="tab-entity">
                            <H5>Type Name</H5>
                            <p>{node.discriminatorTitle}</p>
                        </div>
                        <div className="tab-entity">
                            <H5>Alias</H5>
                            <p>{node.alias}</p>
                        </div>
                        <div className="tab-entity">
                            <H5>Status</H5>
                            <p>{node.published ? 'Published' : 'Not published'}</p>
                        </div>
                        <div className="tab-entity">
                            <H5>View in the site map</H5>
                            <p>{node.isInSiteMap ? 'Yes' : 'No'}</p>
                        </div>
                        <div className="tab-entity">
                            <H5>Visible</H5>
                            <p>{node.isVisible ? 'Yes' : 'No'}</p>
                        </div>
                        <ExtantionCard node={node} isEditMode={isEditMode} />
                    </div>
                </div>
            );
        }

        return null;
    }
}
