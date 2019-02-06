
module Models {

    // $Classes/Enums/Interfaces(filter)[template][separator]
    // filter (optional): Matches the name or full name of the current item. * = match any, wrap in [] to match attributes or prefix with : to match interfaces or base classes.
    // template: The template to repeat for each matched item
    // separator (optional): A separator template that is placed between all templates e.g. $Properties[public $name: $Type][, ]
    
    // More info: http://frhagn.github.io/Typewriter/
    
    
    export class RegionViewModel {
        
        // ID
        public id: number = 0;
        // ALIAS
        public alias: string = null;
        // PARENTID
        public parentId: number = null;
        // TITLE
        public title: string = null;
        // CHILDREN
        public children: RegionViewModel[] = [];
        // HASCHILDREN
        public hasChildren: boolean = false;
    }
}
