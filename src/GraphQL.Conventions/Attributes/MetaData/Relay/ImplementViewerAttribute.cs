using System;
using System.Linq;
using System.Reflection;
using GraphQL.Conventions.Attributes;
using GraphQL.Conventions.Types.Descriptors;
using Namotion.Reflection;

namespace GraphQL.Conventions.Relay
{
    [AttributeUsage(Types, AllowMultiple = true, Inherited = true)]
    public class ImplementViewerAttribute : MetaDataAttributeBase
    {
        private readonly OperationType _operationType;

        public ImplementViewerAttribute(OperationType operationType)
            : base(AttributeApplicationPhase.Override)
        {
            _operationType = operationType;
        }

        public override void MapType(GraphTypeInfo type, ContextualType contextualType)
        {
            ContextualType viewer;
            ContextualType viewerReferrer;

            switch (_operationType)
            {
                case OperationType.Query:
                    viewer = typeof(QueryViewer).ToContextualType();
                    viewerReferrer = typeof(QueryViewerReferrer).ToContextualType();
                    break;
                case OperationType.Mutation:
                    viewer = typeof(MutationViewer).ToContextualType();
                    viewerReferrer = typeof(MutationViewerReferrer).ToContextualType();
                    break;
                case OperationType.Subscription:
                    viewer = typeof(SubscriptionViewer).ToContextualType();
                    viewerReferrer = typeof(SubscriptionViewerReferrer).ToContextualType();
                    break;
                default:
                    return;
            }

            var viewerType = type.TypeResolver.DeriveType(viewer);
            viewerType.Fields.AddRange(type.Fields);
            viewerType.Fields = viewerType.Fields.OrderBy(f => f.Name).ToList();

            var viewerReferrerType = type.TypeResolver.DeriveType(viewerReferrer);
            var viewerField = viewerReferrerType.Fields.First(field => field.Name == "viewer");
            type.Fields.Add(viewerField);
            type.Fields = type.Fields.OrderBy(f => f.Name).ToList();
        }

        public class QueryViewer
        {
        }

        public class QueryViewerReferrer
        {
            public QueryViewer Viewer => new QueryViewer();
        }

        public class MutationViewer
        {
        }

        public class MutationViewerReferrer
        {
            public MutationViewer Viewer => new MutationViewer();
        }

        public class SubscriptionViewer
        {
        }

        public class SubscriptionViewerReferrer
        {
            public SubscriptionViewer Viewer => new SubscriptionViewer();
        }
    }
}
