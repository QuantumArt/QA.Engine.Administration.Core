import React from 'react';
import BaseSelect from './BaseSelect';
import { observer } from 'mobx-react';

interface WidgetIdSelectModel {
    id: number;
    title: string;
  }

@observer
export default class WidgetIdSelector extends BaseSelect<WidgetIdSelectModel> { }
