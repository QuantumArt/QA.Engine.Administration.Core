using AutoMapper;
using NLog.Layouts;
using QA.Engine.Administration.Logger.Core.Models;

namespace QA.Engine.Administration.Logger.Core.Converter
{
    class JsonAttributeTypeConverter : ITypeConverter<JsonField, JsonAttribute>
    {
        public JsonAttribute Convert(JsonField source, JsonAttribute destination, ResolutionContext context)
        {
            var target = destination;
            if (target == null)
            {
                target = source.Layout == null
                    ? new JsonAttribute(source.Name, source.Value)
                    : new JsonAttribute(source.Name, Mapper.Map<JsonLayoutField, JsonLayout>(source.Layout), false);
            }
            return target;
        }
    }
}
