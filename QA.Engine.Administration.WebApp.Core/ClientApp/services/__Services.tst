${
    // Enable extension methods by adding using Typewriter.Extensions.*
    using System.Collections.Concurrent;
    using System.Text;
    using System.Text.RegularExpressions;
    using Typewriter.Extensions.Types;
    using Typewriter.Extensions.WebApi;

    // Uncomment the constructor to change template settings.
    Template(Settings settings)
    {
        settings.OutputFilenameFactory = f => f.Name.Replace("Controller.cs", "Service.ts");
        settings.OutputExtension = ".ts";
    }

    private static Regex WordRegex = new Regex(@"\w+", RegexOptions.Compiled);

    private static ConcurrentDictionary<string, string> ResolvedNameCache = new ConcurrentDictionary<string, string>();

    public string ResolvedName(Type type) => ResolvedNameCache.GetOrAdd(type.Name, _ =>
    {
        var excludedNames = new HashSet<string>();

        CollectExcludedTypes(excludedNames, type);
        
        return WordRegex.Replace(type.Name, m => excludedNames.Contains(m.Value) ? "any" : m.Value);
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

    private bool TypeIsExcluded(Type type)
    {
        return type.IsDefined
            && type.FullName.StartsWith(type.Namespace)
            && !type.Attributes.Any(a => a.Name == "TypeScriptType");
    }

    public string ParamType(Parameter parameter)
    {
        return ResolvedName(parameter.Type);
    }

    public string ReturnType(Method method)
    {
        if (method.Type.Name == "IHttpActionResult" || method.Type.Name == "void")
        {
            return "Response";
        }
        return ResolvedName(method.Type);
    }

    public string ReturnType1(Method method)
    {
        var excluded = TypeIsExcluded(method.Type);
        return $"{method.Type.Name} is excluded: {excluded}";
    }

    public bool HasBody(Method method)
    {
        string httpMethod = method.HttpMethod();
        return httpMethod != "get" && httpMethod != "head";
    }

    public string ServiceName(Class klass)
    {
        return klass.Name.Replace("Controller", "Service");
    }

    public bool HasParameters(Method method)
    {
        string httpMethod = method.HttpMethod();
        return (httpMethod == "get" || httpMethod == "head") && method.Parameters.Any();
    }

    public string GetParameter(Parameter parameter)
    {
        var nullable = parameter.Type.IsNullable ? "?" : "";
        var defaultValue = parameter.HasDefaultValue ? $" = {parameter.DefaultValue}" : "";
        return $"{parameter.name}{nullable}: {parameter.Type}{defaultValue}";
    }

    public string ConstructUrlParams(Parameter parameter)
    {
        if (parameter.Type.IsNullable || parameter.DefaultValue == "null")
            return $"urlparams += Array.isArray({parameter.name}) && {parameter.name}.length === 0 ? '' : ({parameter.name} == null ? '' : `&regionIds=${{{parameter.name}}} `);";
        return $"urlparams += Array.isArray({parameter.name}) && {parameter.name}.length === 0 ? '' : `&{parameter.name}=${{{parameter.name}}} `;";
    }

    public string CustomUrl(Method method)
    {
        var url = method.Url();
        var idx = url.IndexOf("?");
        if (idx < 0)
            return $"const path = '/{url}';";
        url = $"const path = `/{url.Substring(0, idx)}${{urlparams}}`;";
        return url;
    }

    // $Classes/Enums/Interfaces(filter)[template][separator]
    // filter (optional): Matches the name or full name of the current item. * = match any, wrap in [] to match attributes or prefix with : to match interfaces or base classes.
    // template: The template to repeat for each matched item
    // separator (optional): A separator template that is placed between all templates e.g. $Properties[public $name: $Type][, ]

    // More info: http://frhagn.github.io/Typewriter/

}/****************************************************************************
  Generated by TypeWriter - don't make any changes in this file
****************************************************************************/
$Classes([TypeScriptController])[
$DocComment[/** $Summary */
]class $ServiceName {
$Methods(m => !m.Attributes.Any(a => a.Name == "TypeScriptIgnore"))[
$DocComment[    /** $Summary */]
    public async $name($Parameters[$GetParameter][, ]): Promise<$ReturnType> {

        $HasParameters[let urlparams = '';
        $Parameters[$ConstructUrlParams][
        ]
        urlparams = urlparams.length > 0 ? `?${urlparams.slice(1)}` : '';
        ]$CustomUrl
        const headers = new Headers();
        headers.append('Qp-Site-Params', JSON.stringify(this.getHeaderData()));$HasBody[
        headers.append('Content-Type', 'application/json');]
        const init = {
            headers,
            method: '$HttpMethod',$HasBody[
            body: JSON.stringify($RequestData),]
        };

        console.debug(`%cstart api request $HttpMethod '${path}'`, 'color: green;'$HasBody[, $RequestData]);
        const response = await fetch(path, init);

        const result = await <Promise<$ReturnType>>response.json();
        console.log(`%cresult api $HttpMethod '${path}'`, 'color: blue;', result);

        return result;
    }][
]

    private getHeaderData(): any {
        const getQueryVariable = (variable: string) => {
            const result = window.location.search.substring(1).split('&')
                .map(x => ({ name: x.split('=')[0], value: x.split('=')[1] }))
                .filter(x => x.name === variable)[0];
            return result == null ? null : result.value;
        };

        return {
            BackendSid: getQueryVariable('backend_sid'),
            CustomerCode: getQueryVariable('customerCode'),
            HostId: getQueryVariable('hostUID'),
            SiteId: getQueryVariable('site_id'),
        };
    }

}

export default new $ServiceName();]
