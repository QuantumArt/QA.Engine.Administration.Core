
module Models {

    // $Classes/Enums/Interfaces(filter)[template][separator]
    // filter (optional): Matches the name or full name of the current item. * = match any, wrap in [] to match attributes or prefix with : to match interfaces or base classes.
    // template: The template to repeat for each matched item
    // separator (optional): A separator template that is placed between all templates e.g. $Properties[public $name: $Type][, ]
    
    // More info: http://frhagn.github.io/Typewriter/
    
    
    export class DiscriminatorViewModel {
        
        // ID
        public id: number = 0;
        // ISARCHIVE
        public isArchive: boolean = false;
        // DISCRIMINATOR
        public discriminator: string = null;
        // TYPENAME
        public typeName: string = null;
        // ISPAGE
        public isPage: boolean = false;
        // TITLE
        public title: string = null;
        // DESCRIPTION
        public description: string = null;
        // ICONURL
        public iconUrl: string = null;
        // PREFERREDCONTENTID
        public preferredContentId: number = null;
    }
}
