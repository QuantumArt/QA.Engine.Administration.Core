import * as React from 'react';
import { observer, inject } from 'mobx-react';
import { Button, Navbar, NavbarGroup, H5, Intent, Spinner, InputGroup } from '@blueprintjs/core';
import { ITabData } from 'stores/TabsStore';
import { SiteTreeState } from 'stores/SiteTreeStore';
import ExtantionCard from './ExtensionCard';
import { ExtensionFieldsState } from 'stores/ExtensionFieldsStore';
import OperationState from 'enums/OperationState';
import { ArchiveState } from 'stores/ArchiveStore';

interface Props {
    data: ITabData;
    siteTreeStore?: SiteTreeState;
    archiveStore?: ArchiveState;
    extensionFieldsStore?: ExtensionFieldsState;
}

interface ICommonTabState {
    value: PageModel | ArchiveModel;
}
interface State {
    isEditMode: boolean;
    title: string;
    node: ICommonTabState;
}

@inject('siteTreeStore', 'archiveStore', 'extensionFieldsStore')
@observer
export default class CommonTab extends React.Component<Props, State> {

    state = { isEditMode: false, title: '', node: { value: null } as ICommonTabState };

    private getNodeData = (id: number): PageModel | ArchiveModel => {
        const { siteTreeStore, archiveStore } = this.props;
        if (id == null) {
            return null;
        }
        let node: PageModel | ArchiveModel;
        node = siteTreeStore.getNodeById(id);
        if (node == null) {
            node = archiveStore.getNodeById(id);
        }
        return node;
    }

    componentWillReceiveProps(nextProps: Props) {
        const getId = (data: ITabData) => (data || {} as ITabData).id;
        const id = getId(nextProps.data);
        if (id == null) {
            return;
        }
        if (getId(this.props.data) === getId(nextProps.data)) {
            this.setState({ isEditMode: false, node: { value: null } });
        } else {
            const node: PageModel | ArchiveModel = this.getNodeData(+id);
            this.setState({ title: node == null ? null : node.title, isEditMode: false, node: { value: node } });
        }
    }

    private refreshClick = () => {
        const { siteTreeStore, data } = this.props;
        siteTreeStore.updateSubTree(+data.id).then(() => {
            const node: PageModel | ArchiveModel = this.getNodeData(+data.id);
            this.setState({ title: node.title, isEditMode: false, node: { value: node } });
        });
    }

    private editClick = () => {
        this.setState({ isEditMode: true });
    }

    private saveClick = () => {
        const { siteTreeStore, extensionFieldsStore, data } = this.props;
        const { title, node } = this.state;
        const model: EditModel = {
            title,
            itemId: +data.id,
            extensionId: node.value.extensionId,
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
        const { siteTreeStore } = this.props;
        const { isEditMode, title, node } = this.state;
        if (siteTreeStore.treeState === OperationState.NONE || siteTreeStore.treeState === OperationState.PENDING) {
            return (<Spinner size={60} />);
        }
        if (node !== null && node.value != null) {
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
                            <p>{node.value.id}</p>
                        </div>
                        <div className="tab-entity">
                            <H5>Title</H5>
                            {isEditMode ? (<InputGroup value={title} onChange={this.changeTitle} />) : (<p>{title}</p>)}
                        </div>
                        <div className="tab-entity">
                            <H5>Type Name</H5>
                            <p>{node.value.discriminatorTitle}</p>
                        </div>
                        <div className="tab-entity">
                            <H5>Alias</H5>
                            <p>{node.value.alias}</p>
                        </div>
                        <div className="tab-entity">
                            <H5>Status</H5>
                            <p>{node.value.published ? 'Published' : 'Not published'}</p>
                        </div>
                        <div className="tab-entity">
                            <H5>View in the site map</H5>
                            <p>{node.value.isInSiteMap ? 'Yes' : 'No'}</p>
                        </div>
                        <div className="tab-entity">
                            <H5>Visible</H5>
                            <p>{node.value.isVisible ? 'Yes' : 'No'}</p>
                        </div>
                        <ExtantionCard node={node.value} isEditMode={isEditMode} />
                    </div>
                </div>
            );
        }

        return null;
    }
}
