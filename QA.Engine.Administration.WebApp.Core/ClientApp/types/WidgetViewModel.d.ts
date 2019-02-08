﻿/****************************************************************************
  Generated by TypeWriter - don't make any changes in this file
****************************************************************************/

declare interface WidgetViewModel {
  id: number;
  isArchive: boolean;
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
  iconUrl: string;
  children: WidgetViewModel[];
  regions: RegionViewModel[];
  hasChildren: boolean;
  hasRegions: boolean;
}
