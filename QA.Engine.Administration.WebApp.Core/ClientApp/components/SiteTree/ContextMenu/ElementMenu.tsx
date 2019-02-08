import * as React from 'react';
import { Intent, Menu, MenuDivider, MenuItem } from '@blueprintjs/core';
import { observer } from 'mobx-react';

interface Props {

}

@observer
export default class ElementMenu extends React.Component<Props> {
    private handleClick = (e: React.MouseEvent<HTMLElement>, cb: () => void) => {
        e.stopPropagation();
        cb();
    }

    private handlerExample = () => {
        // example method for menu action. Will be taken from some store in the future.
        console.log('click');
    }

    render() {
        return (
            <Menu>
                <MenuItem
                    onClick={(e: React.MouseEvent<HTMLElement>) => this.handleClick(e, this.handlerExample)}
                    icon="refresh"
                    text="Обновить"
                />
                <MenuItem
                    onClick={(e: React.MouseEvent<HTMLElement>) => this.handleClick(e, this.handlerExample)}
                    icon="eye-open"
                    text="Просмотр"
                />
                <MenuItem
                    onClick={(e: React.MouseEvent<HTMLElement>) => this.handleClick(e, this.handlerExample)}
                    icon="history"
                    text="История изменений"
                />
                <MenuDivider/>
                <MenuItem
                    onClick={(e: React.MouseEvent<HTMLElement>) => this.handleClick(e, this.handlerExample)}
                    icon="confirm"
                    text="Публиковать"
                    intent={Intent.SUCCESS}
                />
                <MenuDivider/>
                <MenuItem
                    onClick={(e: React.MouseEvent<HTMLElement>) => this.handleClick(e, this.handlerExample)}
                    icon="new-object"
                    text="Добавить подраздел"
                    intent={Intent.PRIMARY}/>
                <MenuItem
                    onClick={(e: React.MouseEvent<HTMLElement>) => this.handleClick(e, this.handlerExample)}
                    icon="add"
                    text="Добавить версию"
                    intent={Intent.PRIMARY}/>
                <MenuItem
                    onClick={(e: React.MouseEvent<HTMLElement>) => this.handleClick(e, this.handlerExample)}
                    icon="edit"
                    text="Редактировать"
                    intent={Intent.PRIMARY}/>
                <MenuDivider/>
                <MenuItem
                    onClick={(e: React.MouseEvent<HTMLElement>) => this.handleClick(e, this.handlerExample)}
                    icon="box"
                    text="Архивировать"
                    intent={Intent.DANGER}/>
            </Menu>
        );
    }
}
