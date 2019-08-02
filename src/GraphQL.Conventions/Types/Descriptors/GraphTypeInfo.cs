using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using GraphQL.Conventions.Types.Resolution;
using GraphQL.Conventions.Types.Resolution.Extensions;
using Namotion.Reflection;

namespace GraphQL.Conventions.Types.Descriptors
{
    public class GraphTypeInfo : GraphEntityInfo
    {
        private readonly List<GraphTypeInfo> _interfaces = new List<GraphTypeInfo>();

        private readonly List<GraphTypeInfo> _possibleTypes = new List<GraphTypeInfo>();

        private readonly List<GraphTypeInfo> _unionTypes = new List<GraphTypeInfo>();

        public GraphTypeInfo(ITypeResolver typeResolver, ContextualType type)
            : base(typeResolver, type.Type)
        {
            ContextualType = type;
            DeriveMetaData();
        }

        public bool IsRegisteredType { get; set; }

        public bool IsPrimitive { get; set; }

        public bool IsNullable { get; set; }

        public bool IsTask { get; set; }

        public bool IsOutputType { get; set; }

        public bool IsInputType { get; set; }

        public bool IsScalarType => IsOutputType && IsInputType;

        public bool IsInterfaceType { get; private set; }

        public bool IsUnionType { get; set; }

        public bool IsEnumerationType { get; set; }

        public bool IsListType { get; set; }

        public bool IsArrayType { get; set; }

        public bool IsObservable { get; set; }

        public List<GraphTypeInfo> Interfaces => _interfaces;

        public List<GraphTypeInfo> PossibleTypes => _possibleTypes;

        public List<GraphTypeInfo> UnionTypes => IsUnionType ? PossibleTypes : new List<GraphTypeInfo>();

        public List<GraphFieldInfo> Fields { get; internal set; } = new List<GraphFieldInfo>();

        public ContextualType ContextualType { get; set; }

        public TypeInfo TypeRepresentation => ContextualType.TypeInfo;

        public GraphTypeInfo TypeParameter { get; set; }

        public object DefaultValue =>
            TypeRepresentation.IsValueType && !IsNullable && !TypeRepresentation.IsGenericType(typeof(NonNull<>))
                ? Activator.CreateInstance(TypeRepresentation.AsType())
                : null;

        public void AddInterface(GraphTypeInfo typeInfo)
        {
            _interfaces.Add(typeInfo);
            typeInfo.AddPossibleType(this);

            if (typeInfo.IsInputType && !typeInfo.IsOutputType)
            {
                IsInputType = true;
                IsOutputType = false;
            }
        }

        public void AddPossibleType(GraphTypeInfo typeInfo)
        {
            _possibleTypes.Add(typeInfo);
        }

        public void AddUnionType(GraphTypeInfo typeInfo)
        {
            IsUnionType = true;
            AddPossibleType(typeInfo);
        }

        public override string ToString() => $"{nameof(GraphTypeInfo)}:{Name}";

        private void DeriveMetaData()
        {
            var type = ContextualType;
            var typeInfo = type.Type.GetTypeInfo();

            if (typeInfo.IsGenericType(typeof(IObservable<>))) 
            {
                IsObservable = true;
                type = type.TypeParameter();
            }

            if (typeInfo.IsGenericType(typeof(Task<>)))
            {
                IsTask = true;
                type = type.TypeParameter();
            }

            if (typeInfo.IsGenericType(typeof(Nullable<>)))
            {
                IsNullable = true;
                type = type.TypeParameter();
                IsPrimitive = typeInfo.IsPrimitiveGraphType();
            }
            else if (typeInfo.IsGenericType(typeof(NonNull<>)))
            {
                IsNullable = false;
                type = type.TypeParameter();
                IsPrimitive = typeInfo.IsPrimitiveGraphType();
            }
            else if (typeInfo.IsGenericType(typeof(Optional<>)))
            {
                IsNullable = ContextualType.Nullability == Nullability.Nullable;
                type = type.TypeParameter();
                if (typeInfo.IsGenericType(typeof(Nullable<>)))
                {
                    type = type.TypeParameter();
                }
                IsPrimitive = typeInfo.IsPrimitiveGraphType();
            }
            else if (ContextualType.Nullability == Nullability.Nullable)
            {
                IsNullable = true;
                IsPrimitive = typeInfo.IsPrimitiveGraphType();
            }
            else if (ContextualType.Nullability == Nullability.NotNullable)
            {
                IsNullable = false;
                IsPrimitive = typeInfo.IsPrimitiveGraphType();
            }
            else
            {
                IsNullable = !typeInfo.IsValueType;
                IsPrimitive = typeInfo.IsPrimitiveGraphType();
            }

            if (typeInfo.IsEnumerableGraphType())
            {
                IsListType = true;
                IsArrayType = typeInfo.IsArray;
                IsPrimitive = true;
                TypeParameter = TypeResolver.DeriveType(ContextualType.TypeParameter());
            }
            else
            {
                IsListType = false;
            }

            var typeRegistration = TypeResolver.LookupType(typeInfo);
            IsRegisteredType = typeRegistration != null;
            IsOutputType = true;
            IsInputType = IsPrimitive || typeInfo.IsValueType || (typeRegistration?.IsScalar ?? false);
            IsInterfaceType = !IsListType && typeInfo.IsInterface;
            IsEnumerationType = typeInfo.IsEnum;
            Name = typeRegistration?.Name;
        }
    }
}
