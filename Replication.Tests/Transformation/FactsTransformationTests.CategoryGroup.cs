﻿using NUnit.Framework;

// ReSharper disable PossibleUnintendedReferenceComparison
namespace NuClear.AdvancedSearch.Replication.Tests.Transformation
{
    using CI = CustomerIntelligence.Model;
    using Erm = CustomerIntelligence.Model.Erm;
    using Facts = CustomerIntelligence.Model.Facts;

    [TestFixture]
    internal partial class FactTransformationTests
    {
        [Test]
        public void ShouldInitializeCategoryGroupIfCategoryGroupCreated()
        {
            ErmDb.Has(
                new Erm::CategoryGroup { Id = 1, Name = "Name", Rate = 1 });

            Transformation.Create(Query, RepositoryFactory)
                          .ApplyChanges<Facts::CategoryGroup>(1)
                          .VerifyDistinct(Aggregate.Initialize<CI::CategoryGroup>(1));
        }

        [Test]
        public void ShouldDestroyCategoryGroupIfCategoryGroupDeleted()
        {
            FactsDb.Has(
                new Facts::CategoryGroup { Id = 1, Name = "Name", Rate = 1 });

            Transformation.Create(Query, RepositoryFactory)
                          .ApplyChanges<Facts::CategoryGroup>(1)
                          .VerifyDistinct(Aggregate.Destroy<CI::CategoryGroup>(1));
        }

        [Test]
        public void ShouldRecalculateCategoryGroupIfCategoryGroupUpdated()
        {
            ErmDb.Has(
                new Erm::CategoryGroup { Id = 1, Name = "FooBar", Rate = 2 });
            FactsDb.Has(
                new Facts::CategoryGroup { Id = 1, Name = "Name", Rate = 1 });

            Transformation.Create(Query, RepositoryFactory)
                          .ApplyChanges<Facts::CategoryGroup>(1)
                          .VerifyDistinct(Aggregate.Recalculate<CI::CategoryGroup>(1));
        }

        [Test]
        public void ShouldRecalculateClientAndFirmIfCategoryGroupUpdated()
        {
            ErmDb.Has(new Erm::CategoryGroup { Id = 1, Name = "Name", Rate = 1 })
                 .Has(new Erm::CategoryOrganizationUnit { Id = 1, CategoryGroupId = 1, CategoryId = 1, OrganizationUnitId = 1 })
                 .Has(new Erm::CategoryFirmAddress { Id = 1, FirmAddressId = 1, CategoryId = 1 })
                 .Has(new Erm::FirmAddress { Id = 1, FirmId = 1 })
                 .Has(new Erm::Firm { Id = 1, OrganizationUnitId = 1, ClientId = 1 })
                 .Has(new Erm::Client { Id = 1 });


            FactsDb.Has(new Facts::CategoryGroup { Id = 1, Name = "Name", Rate = 1 })
                   .Has(new Facts::CategoryOrganizationUnit { Id = 1, CategoryGroupId = 1, CategoryId = 1, OrganizationUnitId = 1 })
                   .Has(new Facts::CategoryFirmAddress { Id = 1, FirmAddressId = 1, CategoryId = 1 })
                   .Has(new Facts::FirmAddress { Id = 1, FirmId = 1 })
                   .Has(new Facts::Firm { Id = 1, OrganizationUnitId = 1, ClientId = 1 })
                   .Has(new Facts::Client { Id = 1 });

            Transformation.Create(Query, RepositoryFactory)
                          .ApplyChanges<Facts::CategoryGroup>(1)
                          .VerifyDistinct(Aggregate.Recalculate<CI::Firm>(1),
                                          Aggregate.Recalculate<CI::Client>(1),
                                          Aggregate.Recalculate<CI::CategoryGroup>(1));
        }
    }
}