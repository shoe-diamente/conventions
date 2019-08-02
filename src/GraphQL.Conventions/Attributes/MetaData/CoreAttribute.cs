using System;
using System.Reflection;
using GraphQL.Conventions.Attributes.Collectors;
using GraphQL.Conventions.Types.Descriptors;
using GraphQL.Conventions.Types.Resolution.Extensions;
using Namotion.Reflection;

namespace GraphQL.Conventions.Attributes.MetaData
{
    [AttributeUsage(Everywhere, AllowMultiple = true, Inherited = true)]
    public class CoreAttribute : MetaDataAttributeBase, IDefaultAttribute
    {
        public CoreAttribute()
            : base(AttributeApplicationPhase.Initialization)
        {
        }

        public override void MapType(GraphTypeInfo type, ContextualType contextualType)
        {
            var typeRepresentation = contextualType.Type.GetTypeRepresentation();
            if (typeRepresentation.IsSubclassOf(typeof(Union)))
            {
                DeclareUnionType(type, contextualType);
            }
        }

        private void DeclareUnionType(GraphTypeInfo entity, ContextualType contextualType)
        {
            var unionType = contextualType.Type.BaseType.GetTypeInfo();
            if (unionType != null &&
                unionType.Name.StartsWith(nameof(Union), StringComparison.Ordinal) &&
                unionType.IsSubclassOf(typeof(Union)) &&
                unionType.IsGenericType)
            {
                foreach (var type in unionType.GenericTypeArguments)
                {
                    entity.AddUnionType(entity.TypeResolver.DeriveType(contextualType));
                }
            }
        }
    }
}
