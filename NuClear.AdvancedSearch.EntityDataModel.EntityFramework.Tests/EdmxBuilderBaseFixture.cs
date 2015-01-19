using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Linq;

using Effort.Provider;

using Moq;

using NuClear.AdvancedSearch.EntityDataModel.Metadata;
using NuClear.EntityDataModel.EntityFramework.Building;
using NuClear.Metamodeling.Elements;
using NuClear.Metamodeling.Elements.Identities;
using NuClear.Metamodeling.Processors;
using NuClear.Metamodeling.Provider;
using NuClear.Metamodeling.Provider.Sources;

using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace EntityDataModel.EntityFramework.Tests
{
    internal class EdmxBuilderBaseFixture
    {
        [TestFixtureSetUp]
        public void FixtureSetup()
        {
            EffortProviderConfiguration.RegisterProvider();
        }

        protected static DbProviderFactory DefaultFactory
        {
            get
            {
                return DbProviderFactories.GetFactory(EffortProviderConfiguration.ProviderInvariantName);
            }
        }

        protected static DbProviderInfo DefaultProviderInfo
        {
            get
            {
                return new DbProviderInfo(EffortProviderConfiguration.ProviderInvariantName, EffortProviderManifestTokens.Version1);
            }
        }

        protected static EdmxModelBuilder CreateModelBuilder(ITypeProvider typeProvider = null)
        {
            return new EdmxModelBuilder(DefaultProviderInfo, typeProvider);
        }

        protected static DbModel BuildModel(IMetadataSource source, Uri id)
        {
            var builder = CreateModelBuilder();
            var model = builder.Build(CreateProvider(source), id);

            model.Dump();

            return model;
        }

        protected static DbModel BuildModel(BoundedContextElement context)
        {
            var builder = CreateModelBuilder();
            var model = builder.Build(ProcessContext(context));

            model.Dump();

            return model;
        }

        protected static EdmModel BuildConceptualModel(BoundedContextElement context)
        {
            var model = CreateModel(ProcessContext(context));

            model.ConceptualModel.Dump(EdmxExtensions.EdmModelType.Conceptual);

            return model.ConceptualModel;
        }

        protected static EdmModel BuildStoreModel(BoundedContextElement context)
        {
            var model = CreateModel(ProcessContext(context));

            model.StoreModel.Dump(EdmxExtensions.EdmModelType.Store);

            return model.StoreModel;
        }

        protected static BoundedContextElementBuilder NewContext(string name)
        {
            return BoundedContextElement.Config.Name(name);
        }

        protected static StructuralModelElementBuilder NewModel(params EntityElementBuilder[] entities)
        {
            var model = StructuralModelElement.Config;

            foreach (var entityElementBuilder in entities)
            {
                model.Elements(entityElementBuilder);
            }

            return model;
        }

        protected static EntityElementBuilder NewEntity(string name, params EntityPropertyElementBuilder[] properties)
        {
            var config = EntityElement.Config.Name(name);

            if (properties.Length == 0)
            {
                config.Property(NewProperty("Id").NotNull()).IdentifyBy("Id");
            }

            foreach (var propertyElementBuilder in properties)
            {
                config.Property(propertyElementBuilder);
            }

            return config;
        }

        protected static EntityPropertyElementBuilder NewProperty(string propertyName, EntityPropertyType propertyType = EntityPropertyType.Int64)
        {
            return EntityPropertyElement.Config.Name(propertyName).OfType(propertyType);
        }

        protected static EntityRelationElementBuilder NewRelation(string relationName)
        {
            return EntityRelationElement.Config.Name(relationName);
        }

        protected static ModelMappingElementBuilder NewMapping(params EntityElementBuilder[] entities)
        {
            var mapping = ModelMappingElement.Config;

//            foreach (var entityElementBuilder in entities)
//            {
//                model.Elements(entityElementBuilder);
//            }

            return mapping;
        }

        protected static class ConceptualModel
        {
            public static Constraint IsValid { get { return new ModelValidationConstraint(); } }

            private class ModelValidationConstraint : Constraint
            {
                private const int MaxErrorsToDisplay = 5;
                private IReadOnlyCollection<string> _errors;

                public override bool Matches(object value)
                {
                    var model = value as EdmModel;
                    if (model == null)
                    {
                        throw new ArgumentException("The specified actual value is not a model.", "value");
                    }

                    return model.IsValidCsdl(out _errors);
                }

                public override void WriteDescriptionTo(MessageWriter writer)
                {
                    writer.Write("valid");
                }

                public override void WriteActualValueTo(MessageWriter writer)
                {
                    if (_errors.Count == 0)
                    {
                        return;
                    }

                    writer.WriteLine("The model containing errors:");
                    foreach (var error in _errors.Take(MaxErrorsToDisplay))
                    {
                        writer.WriteMessageLine(2, error);
                    }
                }
            }
        }

        protected static class StoreModel
        {
            public static Constraint IsValid { get { return new ModelValidationConstraint(); } }

            private class ModelValidationConstraint : Constraint
            {
                private const int MaxErrorsToDisplay = 5;
                private IReadOnlyCollection<string> _errors;

                public override bool Matches(object value)
                {
                    var model = value as EdmModel;
                    if (model == null)
                    {
                        throw new ArgumentException("The specified actual value is not a model.", "value");
                    }

                    return model.IsValidSsdl(out _errors);
                }

                public override void WriteDescriptionTo(MessageWriter writer)
                {
                    writer.Write("valid");
                }

                public override void WriteActualValueTo(MessageWriter writer)
                {
                    if (_errors.Count == 0)
                    {
                        return;
                    }
                    
                    writer.WriteLine("The model containing errors:");
                    foreach (var error in _errors.Take(MaxErrorsToDisplay))
                    {
                        writer.WriteMessageLine(2, error);
                    }
                }
            }
        }

        protected static class Property
        {
            public static Predicate<EdmProperty> OfType(PrimitiveTypeKind typeKind)
            {
                return x => x.PrimitiveType.PrimitiveTypeKind == typeKind;
            }

            public static Predicate<EdmProperty> IsKey()
            {
                return x => (x.DeclaringType as EntityType) != null && ((EntityType)x.DeclaringType).KeyMembers.Contains(x);
            }

            public static Predicate<EdmProperty> Members(params string[] names)
            {
                return x => names.OrderBy(_ => _).SequenceEqual(x.EnumType.Members.Select(m => m.Name).OrderBy(_ => _));
            }
        }

        #region Utils

        private static DbModel CreateModel(BoundedContextElement context)
        {
            var builder = CreateModelBuilder();
            var model = builder.Build(ProcessContext(context));

            return model;
        }

        private static IMetadataSource MockSource(IMetadataElement context)
        {
            var source = new Mock<IMetadataSource>();
            source.Setup(x => x.Kind).Returns(new AdvancedSearchIdentity());
            source.Setup(x => x.Metadata).Returns(new Dictionary<Uri, IMetadataElement> { { IdBuilder.For<AdvancedSearchIdentity>(), context } });

            return source.Object;
        }

        private static IMetadataProvider CreateProvider(params IMetadataSource[] sources)
        {
            return new MetadataProvider(sources, new IMetadataProcessor[0]);
        }

        protected static BoundedContextElement ProcessContext(IMetadataElement context)
        {
            var provider = CreateProvider(MockSource(context));

            return provider.Metadata.Metadata.Values.OfType<BoundedContextElement>().FirstOrDefault();
        }

        #endregion
    }
}