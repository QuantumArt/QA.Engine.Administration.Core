import * as React from 'react';
import { inject, observer } from 'mobx-react';
import { AnchorButton, H5, InputGroup, Spinner, TextArea, Checkbox, FormGroup, ControlGroup } from '@blueprintjs/core';
import OperationState from 'enums/OperationState';
import EditArticleStore from 'stores/EditArticleStore';
import TextStore from 'stores/TextStore';
import Texts from 'constants/Texts';
import QpIntegrationStore from 'stores/QpIntegrationStore';

interface Props {
    editArticleStore?: EditArticleStore;
    textStore?: TextStore;
    qpIntegrationStore?: QpIntegrationStore;
}

@inject('editArticleStore', 'textStore', 'qpIntegrationStore')
@observer
export default class ExtentionCard extends React.Component<Props> {

    private showClick = () => {
        const { editArticleStore } = this.props;
        editArticleStore.showExtensionFields();
        editArticleStore.fetchExtensionFields();
    }

    private relationClick = (field: ExtensionFieldModel) => {
        const { qpIntegrationStore } = this.props;
        qpIntegrationStore.link(field.relationExtensionId, field.value);
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
                <Spinner size={30} />
            );
        }

        return (
            <React.Fragment>
                {editArticleStore.fields.map((field, i) => (
                    <FormGroup label={field.fieldName} inline key={i}>
                        {readOnlyField(field, editArticleStore.relatedItems, this.relationClick)}
                    </FormGroup>),
                )}
            </React.Fragment>
        );
    }
}

const readOnlyField = (
    field: ExtensionFieldModel,
    relatedItems: Map<number, string>,
    relationClick: (field: ExtensionFieldModel) => void,
    ): JSX.Element => {
    switch (field.typeName.toLowerCase()) {
        case 'string':
        case 'numeric':
        case 'file':
        case 'image':
            return (<InputGroup readOnly value={field.value == null ? '' : field.value} />);
        case 'boolean':
            return (<Checkbox readOnly checked={field.value == null ? false : field.value} />);
        case 'date':
            const d = field.value == null ? null : new Date(Date.parse(field.value));
            const dValue = d == null ? '' : `${d.getDate()}.${d.getMonth() + 1}.${d.getFullYear()}`;
            return (<InputGroup readOnly value={dValue} />);
        case 'time':
            const t = field.value == null ? null : new Date(Date.parse(field.value));
            const tValue = t == null ? '' : `${t.getHours()}:${t.getMinutes()}`;
            return (<InputGroup readOnly value={tValue} />);
        case 'datetime':
            const dt = field.value == null ? null : new Date(Date.parse(field.value));
            const dtValue = dt == null ? '' : `${dt.getDate()}.${dt.getMonth() + 1}.${dt.getFullYear()} ${dt.getHours()}:${dt.getMinutes()}`;
            return (<InputGroup readOnly value={dtValue} />);
        case 'textbox':
        case 'visualedit':
            return (<TextArea large readOnly value={field.value == null ? '' : field.value} fill />);
        case 'relation':
            const name = relatedItems.get(field.attributeId);
            const text = `(${field.value == null ? '' : field.value}) ${name}`;
            return (<AnchorButton minimal rightIcon="th-derived" text={text} onClick={() => { relationClick(field); }} />);
        case 'dynamic image':
            return null;
        case 'relation many-to-one':
            return null;
        default:
            return null;
    }
};
