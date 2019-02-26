import * as React from 'react';
import { inject, observer } from 'mobx-react';
import { AnchorButton, InputGroup } from '@blueprintjs/core';
import OperationState from 'enums/OperationState';
import EditArticleStore from 'stores/EditArticleStore';
import TextStore from 'stores/TextStore';
import Texts from 'constants/Texts';

interface Props {
    editArticleStore?: EditArticleStore;
    textStore?: TextStore;
}

@inject('editArticleStore', 'textStore')
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
        const { editArticleStore, textStore } = this.props;
        const isLoading = editArticleStore.state === OperationState.NONE || editArticleStore.state === OperationState.PENDING;

        if (!editArticleStore.isShowExtensionFields) {
            return (
                <AnchorButton text={textStore.texts[Texts.showExtensionField]} icon="eye-on" onClick={this.showClick} />
            );
        }

        if (isLoading) {
            return (
                <AnchorButton text={textStore.texts[Texts.showExtensionField]} icon="eye-on" loading={isLoading} onClick={this.showClick} />
            );
        }

        return (
            <div>
                {editArticleStore.fields.map((field, i) => (
                    <div key={i}>
                        <span>{field.fieldName}</span>(<small>{field.typeName}</small>)
                        <InputGroup
                            value={field.value == null ? '' : field.value}
                            onChange={(e: React.ChangeEvent<HTMLInputElement>) => this.change(e, field)}
                        />
                    </div>),
                )}
            </div>
        );
    }
}
