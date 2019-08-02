using System;
using System.Reflection;
using GraphQL.Conventions.Attributes;
using GraphQL.Conventions.Types.Descriptors;
using Namotion.Reflection;

namespace GraphQL.Conventions.Relay
{
    [AttributeUsage(Types, AllowMultiple = true, Inherited = true)]
    public class RelayMutationTypeAttribute : MetaDataAttributeBase
    {
        public RelayMutationTypeAttribute()
            : base(AttributeApplicationPhase.Override)
        {
        }

        public override void MapType(GraphTypeInfo type, ContextualType contextualType)
        {
            foreach (var field in type.Fields)
            {
                field.ExecutionFilters.Add(new RelayMutationAttribute());
            }
        }
    }
}
