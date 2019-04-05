import * as React from 'react';
import { inject, observer } from 'mobx-react';
import { AnchorButton, Checkbox, H5, InputGroup, Intent, Spinner, TextArea } from '@blueprintjs/core';
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

    private relationClick = (relationExtensionId: number, relatedId: number) => {
        const { qpIntegrationStore } = this.props;
        qpIntegrationStore.link(relationExtensionId, relatedId);
    }

    render() {
        const { editArticleStore, textStore } = this.props;
        const isLoading = editArticleStore.state === OperationState.NONE || editArticleStore.state === OperationState.PENDING;

        if (!editArticleStore.isShowExtensionFields) {
            return (
                <AnchorButton
                    text={textStore.texts[Texts.showExtensionField]}
                    icon="search-around"
                    onClick={this.showClick}
                    intent={Intent.PRIMARY}
                />
            );
        }

        if (isLoading) {
            return (
                <Spinner size={30}/>
            );
        }

        return (
            <React.Fragment>
                {editArticleStore.fields.map((field, i) => (
                    <div className="tab-entity" key={i}>
                        <H5>{field.fieldName}</H5>
                        {readOnlyField(field, editArticleStore.relatedItems, editArticleStore.relatedManyToOneItems, this.relationClick)}
                    </div>),
                )}
            </React.Fragment>
        );
    }
}

const readOnlyField = (
    field: ExtensionFieldModel,
    relatedItems: Map<number, string>,
    manyRelatedItems: Map<number, Map<number, string>>,
    relationClick: (relationExtensionId: number, relatedId: number) => void,
): JSX.Element => {
    switch (field.typeName.toLowerCase()) {
        case 'string':
        case 'numeric':
        case 'file':
        case 'image':
            return (<InputGroup readOnly value={field.value == null ? '' : field.value}/>);
        case 'boolean':
            return (<Checkbox readOnly checked={field.value == null ? false : field.value}/>);
        case 'date':
            const d = field.value == null ? null : new Date(Date.parse(field.value));
            const dValue = d == null ? '' : `${d.getDate()}.${d.getMonth() + 1}.${d.getFullYear()}`;
            return (<InputGroup readOnly value={dValue}/>);
        case 'time':
            const t = field.value == null ? null : new Date(Date.parse(field.value));
            const tValue = t == null ? '' : `${t.getHours()}:${t.getMinutes()}`;
            return (<InputGroup readOnly value={tValue}/>);
        case 'datetime':
            const dt = field.value == null ? null : new Date(Date.parse(field.value));
            const dtValue = dt == null ? '' : `${dt.getDate()}.${dt.getMonth() + 1}.${dt.getFullYear()} ${dt.getHours()}:${dt.getMinutes()}`;
            return (<InputGroup readOnly value={dtValue}/>);
        case 'textbox':
        case 'visualedit':
            return (<TextArea rows={10} large readOnly value={field.value == null ? '' : field.value} fill/>);
        case 'relation':
            if (field.value == null || !relatedItems.has(field.attributeId)) {
                return null;
            }
            const name = relatedItems.get(field.attributeId);
            const text = `(${field.value}) ${name == null ? '' : name}`;
            return (<AnchorButton
                minimal
                intent={Intent.PRIMARY}
                rightIcon="th-derived"
                text={text}
                onClick={() => relationClick(field.relationExtensionId, field.value)}
            />);
        case 'dynamic image':
            return null;
        case 'relation many-to-one':
            if (field.value == null || !manyRelatedItems.has(field.attributeId)) {
                return null;
            }
            const map = manyRelatedItems.get(field.attributeId);
            const el = Array.from(map.keys()).map((x) => {
                const name = map.get(x);
                const text = `(${x}) ${name == null ? '' : name}`;
                return (<AnchorButton
                    key={x}
                    intent={Intent.PRIMARY}
                    minimal
                    rightIcon="th-derived"
                    text={text}
                    onClick={() => relationClick(field.relationExtensionId, x)}
                />);
            });
            return (<React.Fragment>{el}</React.Fragment>);
        default:
            return null;
    }
};
