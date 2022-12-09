import * as React from "react";
import {
    MenuItem,
    Button,
    Position,
    Intent,
    IPopoverProps,
} from "@blueprintjs/core";
import { Select, ItemRenderer, ItemPredicate } from "@blueprintjs/select";
import { TreeType } from "stores/TreeStore";
import TreeStoreType from "enums/TreeStoreType";

function getText<T extends { id: number; title: string; alias?: string }>(
    item: T
): string {
    return item.alias == null ? item.title : `${item.title} (${item.alias})`;
}

function renderItem<
    T extends { id: number; title: string; alias?: string }
>(): ItemRenderer<T> {
    return (item, { handleClick, modifiers, query }) => {
        if (!modifiers.matchesPredicate) {
            return null;
        }
        const text = getText(item);
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

function filterItem<
    T extends { id: number; title: string; alias?: string }
>(): ItemPredicate<T> {
    return (query, item) => {
        return getText(item).toLowerCase().indexOf(query.toLowerCase()) >= 0;
    };
}

function highlightText(text: string, query: string) {
    let lastIndex = 0;
    const words = query
        .split(/\s+/)
        .filter((word) => word.length > 0)
        .map(escapeRegExpChars);
    if (words.length === 0) {
        return [text];
    }
    const regexp = new RegExp(words.join("|"), "gi");
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
    return text.replace(/([.*+?^=!:${}()|\[\]\/\\])/g, "\\$1");
}

interface Props<T> {
    items: T[];
    disabled?: boolean;
    filterable?: boolean;
    intent?: Intent;
    onChange: (x: T) => void;
    popoverProps?: IPopoverProps;
    className?: string;
    tree?: TreeType;
    defaultTitle?: string;
}

interface State<T> {
    page: T;
}

export default abstract class BaseSelect<
    T extends { id: number; title: string; alias?: string }
> extends React.Component<Props<T>, State<T>> {
    selectElement = Select.ofType<T>();

    constructor(props: Props<T>) {
        super(props);
        this.state = { page: null };
    }

    private selectItemClick = (item: T) => {
        this.setState({ page: item });
        this.props.onChange(item);
    };

    componentDidUpdate(prevProps: Readonly<Props<T>>): void {
        if (prevProps.items !== this.props.items && this.state.page) {
            const pageInItems = this.props.items.some(
                (item) => item.id === this.state.page.id
            );
            if (
                !pageInItems ||
                (!this.props?.tree?.selectedDiscriminatorsActive &&
                    this.props?.tree?.type === TreeStoreType.WIDGET)
            ) {
                this.setState({
                    page: null,
                });
            }
        }
    }

    render() {
        const {
            items,
            disabled,
            filterable,
            intent,
            popoverProps,
            className,
            defaultTitle,
        } = this.props;
        const { page } = this.state;

        return (
            <this.selectElement
                items={items}
                itemRenderer={renderItem()}
                itemPredicate={filterItem()}
                filterable={filterable === true}
                onItemSelect={this.selectItemClick}
                disabled={disabled}
                popoverProps={{
                    position: Position.BOTTOM,
                    boundary: "viewport",
                    popoverClassName: "select-menu",
                    ...popoverProps,
                }}
                className={className}
            >
                <Button
                    rightIcon="caret-down"
                    text={
                        page === null
                            ? defaultTitle
                                ? defaultTitle
                                : "(No selection)"
                            : page.title
                    }
                    disabled={disabled}
                    intent={intent}
                />
            </this.selectElement>
        );
    }
}
