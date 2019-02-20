import * as React from 'react';
import { MenuItem, Button } from '@blueprintjs/core';
import { Select, ItemRenderer } from '@blueprintjs/select';

function renderDiscriminator<T extends { id: number, title: string }>(): ItemRenderer<T> {
    return (item, { handleClick, modifiers, query }) => {
        if (!modifiers.matchesPredicate) {
            return null;
        }
        const text = item.title;
        return (
            <MenuItem
                active={modifiers.active}
                disabled={modifiers.disabled}
                key={item.id}
                onClick={handleClick}
                text={highlightText(text, query)}
            />
        );
    };
}

function highlightText(text: string, query: string) {
    let lastIndex = 0;
    const words = query
        .split(/\s+/)
        .filter(word => word.length > 0)
        .map(escapeRegExpChars);
    if (words.length === 0) {
        return [text];
    }
    const regexp = new RegExp(words.join('|'), 'gi');
    const tokens: React.ReactNode[] = [];
    while (true) {
        const match = regexp.exec(text);
        if (!match) {
            break;
        }
        const length = match[0].length;
        const before = text.slice(lastIndex, regexp.lastIndex - length);
        if (before.length > 0) {
            tokens.push(before);
        }
        lastIndex = regexp.lastIndex;
        tokens.push(<strong key={lastIndex}>{match[0]}</strong>);
    }
    const rest = text.slice(lastIndex);
    if (rest.length > 0) {
        tokens.push(rest);
    }
    return tokens;
}

function escapeRegExpChars(text: string) {
    return text.replace(/([.*+?^=!:${}()|\[\]\/\\])/g, '\\$1');
}

interface Props<T> {
    items: T[];
    disabled?: boolean;
    onChange: (x: T) => void;
}

interface State<T> {
    page: T;
}

export default abstract class BaseSelect<T extends { id: number, title: string }> extends React.Component<Props<T>, State<T>> {

    selectElement = Select.ofType<T>();

    constructor(props: Props<T>) {
        super(props);
        this.state = { page: null };
    }

    private selectItemClick = (item: T) => {
        this.setState({ page: item });
        this.props.onChange(item);
    }

    render() {
        const { items, disabled } = this.props;
        const { page } = this.state;

        return (
            <this.selectElement
                items={items}
                itemRenderer={renderDiscriminator()}
                itemPredicate={(query, item) => item.title.indexOf(query.toLowerCase()) >= 0}
                filterable={false}
                onItemSelect={this.selectItemClick}
                disabled={disabled}
            >
                <Button
                    rightIcon="caret-down"
                    text={page === null ? '(No selection)' : page.title}
                    disabled={disabled}
                />
            </this.selectElement>
        );
    }
}
