import * as React from 'react';
import { Intent, Menu, MenuDivider, MenuItem } from '@blueprintjs/core';
import { observer } from 'mobx-react';

interface Props {

}

@observer
export default class ElementMenu extends React.Component<Props> {
    render() {
        return (
            <Menu>
                <MenuItem icon="refresh" text="Обновить"/>
                <MenuItem icon="eye-open" text="Просмотр"/>
                <MenuItem icon="history" text="История изменений"/>
                <MenuDivider/>
                <MenuItem icon="confirm" text="Публиковать" intent={Intent.SUCCESS}/>
                <MenuDivider/>
                <MenuItem icon="new-object" text="Добавить подраздел" intent={Intent.PRIMARY}/>
                <MenuItem icon="add" text="Добавить версию" intent={Intent.PRIMARY}/>
                <MenuItem icon="edit" text="Редактировать" intent={Intent.PRIMARY}/>
                <MenuDivider/>
                <MenuItem icon="box" text="Архивировать" intent={Intent.DANGER}/>
            </Menu>
        )
    }
}
