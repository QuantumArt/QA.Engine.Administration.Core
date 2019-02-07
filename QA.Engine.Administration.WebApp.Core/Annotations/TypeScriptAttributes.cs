using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QA.Engine.Administration.WebApp.Core.Annotations
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum)]
    public sealed class TypeScriptType : Attribute
    {
        public bool CamelCase { get; set; }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method)]
    public sealed class TypeScriptIgnore : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Property)]
    public sealed class TypeScriptProperty : Attribute
    {
        public TypeScriptProperty(string name)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class TypeScriptController : Attribute
    {
    }
}
