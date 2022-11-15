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
    selectedDiscriminatorsActive?: boolean;
    defaultTitle?: string;
};

@inject("textStore")
@observer
export default class WidgetIdSelector extends React.Component<Props, State> {
    commonItems = [
        {
            id: SelectorsType.NO_SELECTION,
            title: this.props.textStore.texts[Texts.selectMode],
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
        const { treeSearchActive, selectedDiscriminatorsActive } = this.props;
        if (
            previousProps.treeSearchActive !== this.props.treeSearchActive ||
            previousProps.selectedDiscriminatorsActive !==
                this.props.selectedDiscriminatorsActive
        ) {
            if (treeSearchActive || selectedDiscriminatorsActive) {
                this.setState({
                    items: this.state.items.some(
                        (item) => item.id === SelectorsType.SHOWPATH
                    )
                        ? this.state.items
                        : [...this.state.items, this.showPathItem],
                });
            } else if (!treeSearchActive && !selectedDiscriminatorsActive) {
                const showPathRemoved = this.state.items.filter(
                    (item) => item.id !== SelectorsType.SHOWPATH
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
                defaultTitle={this.props.defaultTitle}
            />
        );
    }
}
