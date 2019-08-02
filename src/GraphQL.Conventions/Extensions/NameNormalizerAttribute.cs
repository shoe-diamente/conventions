using System.Reflection;
using GraphQL.Conventions.Attributes;
using GraphQL.Conventions.Attributes.Collectors;
using GraphQL.Conventions.Types.Descriptors;
using Namotion.Reflection;

namespace GraphQL.Conventions.Extensions
{
    public class NameNormalizerAttribute : MetaDataAttributeBase, IDefaultAttribute
    {
        public NameNormalizerAttribute()
            : base(AttributeApplicationPhase.Override)
        {
        }

        public override void MapType(GraphTypeInfo type, ContextualType contextualType)
        {
            if (type.Name?.EndsWith("Dto") ?? false)
            {
                type.Name = type.Name.Remove(type.Name.Length - 3);
            }
        }
    }
}