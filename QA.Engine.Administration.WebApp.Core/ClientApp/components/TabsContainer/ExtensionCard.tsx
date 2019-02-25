import * as React from 'react';
import { inject, observer } from 'mobx-react';
import { AnchorButton, H5, InputGroup, Spinner } from '@blueprintjs/core';
import OperationState from 'enums/OperationState';
import EditArticleStore from 'stores/EditArticleStore';

interface Props {
    editArticleStore?: EditArticleStore;
}

@inject('editArticleStore')
@observer
export default class ExtentionCard extends React.Component<Props> {

    private showClick = () => {
        const { editArticleStore } = this.props;
        editArticleStore.showExtensionFields();
        editArticleStore.fetchExtantionFields();
    }

    private change = (e: React.ChangeEvent<HTMLInputElement>, field: ExtensionFieldModel) => {
        field.value = e.target.value;
    }

    render() {
        const { editArticleStore } = this.props;
        const isLoading = editArticleStore.state === OperationState.NONE || editArticleStore.state === OperationState.PENDING;

        if (!editArticleStore.isShowExtensionFields) {
            return <AnchorButton text="show extension fields" icon="eye-on" onClick={this.showClick} />;
        }

        if (isLoading) {
            return <Spinner size={30} />;
        }

        return (
            <React.Fragment>
                {editArticleStore.fields.map((field, i) => (
                    <div className="tab-entity" key={i}>
                        <H5 className="extension-header">{field.fieldName}</H5>
                        <small>{field.typeName}</small>
                        <InputGroup
                            value={field.value == null ? '' : field.value}
                            onChange={(e: React.ChangeEvent<HTMLInputElement>) => this.change(e, field)}
                        />
                    </div>),
                )}
            </React.Fragment>
        );
    }
}
