${
    // Enable extension methods by adding using Typewriter.Extensions.*
    using System.Collections.Concurrent;
    using System.Text;
    using System.Text.RegularExpressions;
    using Typewriter.Extensions.Types;

    // Uncomment the constructor to change template settings.
    Template(Settings settings)
    {
        settings.IncludeReferencedProjects();
        settings.IncludeCurrentProject();
        settings.OutputExtension = ".d.ts";
    }

    private bool TypeIsExcluded(Type type)
    {
        return type.IsDefined
            && type.FullName.StartsWith(type.Namespace)
            && !type.Attributes.Any(a => a.Name == "TypeScriptType");
    }

    public bool HasBaseClass(Class klass)
    {
        return klass.BaseClass != null && !TypeIsExcluded(klass.BaseClass);
    }

    public bool HasExcludedBaseClass(Class klass)
    {
        return klass.BaseClass != null && TypeIsExcluded(klass.BaseClass);
    }

    public string ResolvedPropertyName(Property property)
    {
        Attribute tsProperty = property.Attributes
            .FirstOrDefault(a => a.Name == "TypeScriptProperty");

        if (tsProperty != null)
        {
            string name = tsProperty?.Value.Split(',').First();

            if (!String.IsNullOrWhiteSpace(name))
            {
                return NullablePropertyName(property, name);
            }
        }

        Attribute jsonProperty = property.Attributes
            .FirstOrDefault(a => a.Name == "JsonProperty");

        if (jsonProperty != null)
        {
            string name = jsonProperty?.Value.Split(',').First();

            if (!String.IsNullOrWhiteSpace(name))
            {
                return NullablePropertyName(property, name);
            }
        }

        Attribute tsType;
        if(property.Parent is Class){
            tsType = ((Class)property.Parent).Attributes
                .FirstOrDefault(a => a.Name == "TypeScriptType");
        } else{
            tsType = ((Interface)property.Parent).Attributes
                .FirstOrDefault(a => a.Name == "TypeScriptType");
        }

        if (tsType != null && tsType.Value != null && tsType.Value.Contains("CamelCase = true"))
        {
            return NullablePropertyName(property, property.Name);
        }

        return NullablePropertyName(property, property.name);
    }

    private string NullablePropertyName(Property property, string name)
    {
        return property.Type.IsNullable ? name + "?" : name;
    }

    public string ResolvedName(Class klass)
    {
        var sb = new StringBuilder(klass.Name);

        if (klass.TypeArguments.Any())
        {
            sb.Append('<');
            foreach (Type type in klass.TypeArguments)
            {
                sb.Append(ResolvedName(type));
            }
            sb.Append('>');
        }

        return sb.ToString();
    }

    private static Regex WordRegex = new Regex(@"\w+", RegexOptions.Compiled);

    private static ConcurrentDictionary<string, string> ResolvedNameCache = new ConcurrentDictionary<string, string>();

    public string ResolvedName(Type type) => ResolvedNameCache.GetOrAdd(type.FullName, _ =>
    {
        var excludedNames = new HashSet<string>();

        CollectExcludedTypes(excludedNames, type);
        
        string textType = WordRegex.Replace(type.Name, m => excludedNames.Contains(m.Value) ? "any" : m.Value);

        if (type.IsNullable || !type.IsPrimitive && !type.IsEnumerable) {
            return $"null | {textType}";
        }
        return textType;
    });

    private void CollectExcludedTypes(HashSet<string> excludedNames, Type type)
    {
        if (TypeIsExcluded(type))
        {
            excludedNames.Add(type.Name);
        }
        foreach (var parameter in type.TypeArguments)
        {
            CollectExcludedTypes(excludedNames, parameter);
        }
    }

    public bool HasExcludedType(Property property)
    {
        return property.Type.Name != ResolvedName(property.Type);
    }

    public IEnumerable<Property> ResolvedProperties(Class klass)
    {
        return klass.Properties
            .Where(p => p.HasGetter
                && !p.Attributes.Any(a => a.Name == "TypeScrptIgnore" || a.name == "JsonIgnore")
                && !p.Type.Attributes.Any(a => a.Name == "JsonIgnore"));
    }

    public IEnumerable<Property> ResolvedProperties(Interface klass)
    {
        return klass.Properties
            .Where(p => p.HasGetter
                && !p.Attributes.Any(a => a.Name == "TypeScrptIgnore" || a.name == "JsonIgnore")
                && !p.Type.Attributes.Any(a => a.Name == "JsonIgnore"));
    }

    // $Classes/Enums/Interfaces(filter)[template][separator]
    // filter (optional): Matches the name or full name of the current item. * = match any, wrap in [] to match attributes or prefix with : to match interfaces or base classes.
    // template: The template to repeat for each matched item
    // separator (optional): A separator template that is placed between all templates e.g. $Properties[public $name: $Type][, ]

    // More info: http://frhagn.github.io/Typewriter/

}/****************************************************************************
  Generated by TypeWriter - don't make any changes in this file
****************************************************************************/
$Classes([TypeScriptType])[
$DocComment[/** $Summary */
]declare interface $Name$TypeParameters $HasBaseClass[extends $BaseClass[$ResolvedName] ]{$HasExcludedBaseClass[ // extends $BaseClass$TypeParameters]$ResolvedProperties[
$DocComment[  /** $Summary */
]  $ResolvedPropertyName: $Type[$ResolvedName];$HasExcludedType[ // $Type ]]
}
]$Enums([TypeScriptType])[
$DocComment[/** $Summary */
]declare enum $Name {$Values[
$DocComment[  /** $Summary */
]  $Name = $Value,]
}
]
