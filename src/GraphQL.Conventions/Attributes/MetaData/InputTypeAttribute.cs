using System;
using System.Reflection;
using GraphQL.Conventions.Attributes;
using GraphQL.Conventions.Types.Descriptors;
using Namotion.Reflection;

namespace GraphQL.Conventions
{
    [AttributeUsage(Types, AllowMultiple = true, Inherited = true)]
    public class InputTypeAttribute : MetaDataAttributeBase
    {
        public InputTypeAttribute()
            : base(AttributeApplicationPhase.Initialization)
        {
        }

        public override void MapType(GraphTypeInfo type, ContextualType contextualType)
        {
            type.IsInputType = true;
            type.IsOutputType = false;

            foreach (var field in type.Fields)
            {
                if (field.AttributeProvider is MethodInfo)
                {
                    field.IsIgnored = true;
                }
            }
        }
    }
}
