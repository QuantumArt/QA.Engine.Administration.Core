import * as React from 'react';
import { MenuItem, Button } from '@blueprintjs/core';
import { Select, ItemRenderer, ItemPredicate } from '@blueprintjs/select';
import { observer, inject } from 'mobx-react';

interface Props {
    items: PageModel[];
    disabled?: boolean;
    onChange: (x: PageModel) => void;
}

interface State {
    page: PageModel;
}

// tslint:disable-next-line:variable-name
const SelectElement = Select.ofType<PageModel>();

const renderDiscriminator: ItemRenderer<PageModel> = (item, { handleClick, modifiers, query }) => {
    if (!modifiers.matchesPredicate) {
        return null;
    }
    const text = item.title;
    return (
        <MenuItem
            active={modifiers.active}
            disabled={modifiers.disabled}
            label={item.title}
            key={item.id}
            onClick={handleClick}
            text={highlightText(text, query)}
        />
    );
};

const filterDiscriminator: ItemPredicate<PageModel> = (query, film) => {
    return film.title.indexOf(query.toLowerCase()) >= 0;
};

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

@observer
export default class PageSelect extends React.Component<Props, State> {

    constructor(props: any) {
        super(props);
        this.state = { page: null };
    }

    private selectItemClick = (item: PageModel) => {
        this.setState({ page: item });
        this.props.onChange(item);
    }

    render() {
        const { items, disabled } = this.props;
        const { page } = this.state;

        return (
            <SelectElement
                items={items}
                itemRenderer={renderDiscriminator}
                itemPredicate={filterDiscriminator}
                filterable={false}
                onItemSelect={this.selectItemClick}
                disabled={!!disabled}
            >
                <Button
                    rightIcon="caret-down"
                    fill={true}
                    text={page == null ? '(No selection)' : page.title}
                />
            </SelectElement>
        );
    }
}
