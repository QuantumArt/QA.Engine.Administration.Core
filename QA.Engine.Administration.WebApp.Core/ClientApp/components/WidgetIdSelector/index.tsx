import React from "react";
import { inject, observer } from "mobx-react";
import BaseSelect from "components/Select/BaseSelect";
import { TreeType } from "stores/TreeStore";
import TextStore from "stores/TextStore";
import Texts from "constants/Texts";
import TreeStoreType from "enums/TreeStoreType";

interface WidgetIdSelectModel {
    id: number;
    title: string;
    onSelectHandler: () => void;
}

type State = {
    items: {
        site: WidgetIdSelectModel[];
        widget: WidgetIdSelectModel[];
    };
};

type Props = {
    tree: TreeType;
    textStore: TextStore;
};

@observer
export default class WidgetIdSelector extends React.Component<Props, State> {
    commonItems = [
        {
            id: 0,
            title: "(No selection)",
            onSelectHandler: () => {
                this.props.tree.hidePathAndIDs();
            },
        },
        {
            id: 1,
            title: this.props.textStore.texts[Texts.showID] ?? "Показать ID",
            onSelectHandler: () => {
                if (!this.props.tree.showIDs) {
                    this.props.tree.toggleIDs();
                }
            },
        },
    ];

    state = {
        items: {
            widget: [...this.commonItems,
                {
                    id: 2,
                    title: "Показать alias",
                    onSelectHandler: () => {
                        if (!this.props.tree.showAlias) {
                            this.props.tree.toggleAlias();
                        }
                    },
                },
            ],
            site: [
                ...this.commonItems,
                {
                    id: 2,
                    title: "Показать путь",
                    onSelectHandler: () => {
                        if (!this.props.tree.showPath) {
                            this.props.tree.togglePath();
                        }
                    },
                },
            ],
        },
    };

    render() {
        switch (this.props.tree.type) {
            case TreeStoreType.WIDGET:
                return (
                    <BaseSelect
                        items={this.state.items.widget}
                        onChange={(e) => e.onSelectHandler()}
                    />
                );
            case TreeStoreType.SITE:
                return (
                    <BaseSelect
                        items={this.state.items.site}
                        onChange={(e) => e.onSelectHandler()}
                    />
                );
            default:
                return null;
        }
    }
}
