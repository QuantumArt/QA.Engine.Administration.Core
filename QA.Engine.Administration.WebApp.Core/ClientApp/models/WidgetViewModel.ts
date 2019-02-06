
module Models {

    // $Classes/Enums/Interfaces(filter)[template][separator]
    // filter (optional): Matches the name or full name of the current item. * = match any, wrap in [] to match attributes or prefix with : to match interfaces or base classes.
    // template: The template to repeat for each matched item
    // separator (optional): A separator template that is placed between all templates e.g. $Properties[public $name: $Type][, ]
    
    // More info: http://frhagn.github.io/Typewriter/
    
    
    export class WidgetViewModel {
        
        // ID
        public id: number = 0;
        // ISARCHIVE
        public isArchive: boolean = false;
        // PARENTID
        public parentId: number = null;
        // ALIAS
        public alias: string = null;
        // TITLE
        public title: string = null;
        // ZONENAME
        public zoneName: string = null;
        // EXTENSIONID
        public extensionId: number = null;
        // INDEXORDER
        public indexOrder: number = null;
        // ISVISIBLE
        public isVisible: boolean = null;
        // VERSIONOFID
        public versionOfId: number = null;
        // PUBLISHED
        public published: boolean = false;
        // DISCRIMINATORID
        public discriminatorId: number = 0;
        // CHILDREN
        public children: WidgetViewModel[] = [];
        // DISCRIMINATOR
        public discriminator: DiscriminatorViewModel = null;
        // REGIONS
        public regions: RegionViewModel[] = [];
        // HASCHILDREN
        public hasChildren: boolean = false;
        // HASREGIONS
        public hasRegions: boolean = false;
    }
}
