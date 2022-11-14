import React from "react";
import { inject, observer } from "mobx-react";
import BaseSelect from "components/Select/BaseSelect";
import { TreeType } from "stores/TreeStore";
import Texts from "constants/Texts";
import TextStore from "stores/TextStore";
import SelectorsType from "enums/WidgetIdSelectorType";

interface WidgetIdSelectModel {
    id: number;
    title: string;
    onSelectHandler: () => void;
}

type State = {
    items: WidgetIdSelectModel[];
};

type Props = {
    tree: TreeType;
    textStore?: TextStore;
    treeSearchActive?: boolean;
};

@inject("textStore")
@observer
export default class WidgetIdSelector extends React.Component<Props, State> {
    commonItems = [
        {
            id: SelectorsType.NO_SELECTION,
            title: "(No selection)",
            onSelectHandler: () => {
                this.props.tree.hidePathAndIDs();
            },
        },
        {
            id: SelectorsType.SHOWID,
            title: this.props.textStore.texts[Texts.showID],
            onSelectHandler: () => {
                if (!this.props.tree.showIDs) {
                    this.props.tree.toggleIDs();
                }
            },
        },
        {
            id: SelectorsType.SHOWALIAS,
            title: this.props.textStore.texts[Texts.showAlias],
            onSelectHandler: () => {
                if (!this.props.tree.showAlias) {
                    this.props.tree.toggleAlias();
                }
            },
        },
    ];

    showPathItem = {
        id: SelectorsType.SHOWPATH,
        title: this.props.textStore.texts[Texts.showPath],
        onSelectHandler: () => {
            if (!this.props.tree.showPath) {
                this.props.tree.togglePath();
            }
        },
    };

    state = {
        items: this.commonItems,
    };

    componentDidUpdate(previousProps: Props) {
        const { treeSearchActive } = this.props;
        if (previousProps.treeSearchActive !== this.props.treeSearchActive) {
            if (treeSearchActive) {
                this.setState({
                    items: [...this.state.items, this.showPathItem],
                });
            } else if (!treeSearchActive) {
                const showPathRemoved = this.state.items.filter(
                    (item) => item.id !== 3
                );
                if (this.props.tree.showPath) {
                    this.props.tree.togglePath();
                }
                this.setState({
                    items: showPathRemoved,
                });
            }
        }
    }

    render() {
        return (
            <BaseSelect
                items={this.state.items}
                onChange={(e) => {
                    e.onSelectHandler();
                }}
            />
        );
    }
}
