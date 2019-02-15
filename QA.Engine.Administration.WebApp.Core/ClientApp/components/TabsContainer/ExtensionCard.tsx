import * as React from 'react';
import { inject, observer } from 'mobx-react';
import { Spinner, AnchorButton } from '@blueprintjs/core';
import { ExtensionFieldsState } from 'stores/ExtensionFieldsStore';
import OperationState from 'enums/OperationState';

interface Props {
    node: PageModel | ArchiveModel;
    isEditMode: boolean;
    extensionFieldsStore?: ExtensionFieldsState;
}

interface State {
    showItem: boolean;
    itemId?: number;
    fields: ExtensionFieldModel[];
}

@inject('extensionFieldsStore')
@observer
export default class ExtantionCard extends React.Component<Props, State> {
    constructor(props: any) {
        super(props);
        this.state = { showItem: false, itemId: null, fields: [] };
    }

    componentWillReceiveProps(nextProps: Props) {
        const { itemId, showItem } = this.state;
        const { extensionFieldsStore, node, isEditMode } = nextProps;

        if (itemId !== node.id) {
            extensionFieldsStore.state = OperationState.NONE;
            this.setState({ showItem: false, itemId: null, fields: [] });
        }
        if (showItem === true && isEditMode === false) {
            this.setState({ showItem: false, fields: extensionFieldsStore.fields.map(x => Object.assign({}, x)) });
        }
    }

    private showClick = () => {
        const { extensionFieldsStore, node } = this.props;

        this.setState({ showItem: true, itemId: node.id });
        extensionFieldsStore.fetchExtantionFields(node.id, node.extensionId).then(() => {
            this.setState({ fields: extensionFieldsStore.fields.map(x => Object.assign({}, x)) });
        });
    }

    private change = (e: React.ChangeEvent<HTMLInputElement>, field: ExtensionFieldModel) => {
        const { fields } = this.state;
        const { extensionFieldsStore } = this.props;

        field.value = e.target.value;
        extensionFieldsStore.changedFields = fields.filter(x =>
            extensionFieldsStore.fields.filter(y =>
                y.fieldName === x.fieldName && y.value !== x.value).length > 0);
        this.setState({ fields });
    }

    render() {
        const { showItem, fields } = this.state;
        const { extensionFieldsStore, isEditMode } = this.props;
        const isLoading = extensionFieldsStore.state === OperationState.NONE || extensionFieldsStore.state === OperationState.PENDING;

        if (!showItem) {
            return (
                <AnchorButton text="show extension fields" icon="eye-on" onClick={this.showClick} />
            );
        }

        if (isLoading) {
            return (
                <AnchorButton text="show extension fields" icon="eye-on" loading={isLoading} onClick={this.showClick} />
            );
        }

        return (
            <div>
                {
                    fields.map((field, i) => (<p key={i}>
                        <span>{field.fieldName}</span>(<small>{field.typeName}</small>)
                        { isEditMode
                        ? (<input
                            value={field.value == null ? '' : field.value}
                            onChange={(e: React.ChangeEvent<HTMLInputElement>) => this.change(e, field)}
                        />)
                        : field.value
                        }
                    </p>))
                }
            </div>
        );
    }
}
