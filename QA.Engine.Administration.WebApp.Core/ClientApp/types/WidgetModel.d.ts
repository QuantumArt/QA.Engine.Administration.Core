﻿/****************************************************************************
  Generated by TypeWriter - don't make any changes in this file
****************************************************************************/

declare interface WidgetModel {
  id: number;
  archive: boolean;
  visible: boolean;
  parentId?: null | number; // number
  alias: string;
  title: string;
  zoneName: string;
  extensionId?: null | number; // number
  indexOrder?: null | number; // number
  isVisible?: null | boolean; // boolean
  versionOfId?: null | number; // number
  published: boolean;
  discriminator: string;
  discriminatorId: number;
  discriminatorTitle: string;
  children: WidgetModel[];
  regionIds: number[];
  hasChildren: boolean;
  hasRegions: boolean;
}

