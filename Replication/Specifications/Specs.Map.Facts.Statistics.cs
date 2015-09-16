﻿using System.Linq;

using NuClear.Storage.Readings;
using NuClear.Storage.Specifications;

namespace NuClear.AdvancedSearch.Replication.Specifications
{
    using CI = NuClear.AdvancedSearch.Replication.CustomerIntelligence.Model;
    using Facts = NuClear.AdvancedSearch.Replication.CustomerIntelligence.Model.Facts;

    public static partial class Specs
    {
        public static partial class Map
        {
            public static partial class Facts
            {
                public static partial class ToStatistics
                { 
                    public static readonly MapSpecification<IQuery, IQueryable<CI::FirmCategoryStatistics>> FirmCategoryStatistics =
                        new MapSpecification<IQuery, IQueryable<CI::FirmCategoryStatistics>>(
                            q =>
                            {
                                var firmCounts = q.For<Facts::FirmCategory>()
                                                  .GroupBy(x => new { x.ProjectId, x.CategoryId })
                                                  .Select(x => new { x.Key.ProjectId, x.Key.CategoryId, Count = x.Count() });

                                return from firm in q.For<Facts::FirmCategory>()
                                       from firmStatistics in q.For<Facts::FirmCategoryStatistics>().Where(x => x.FirmId == firm.FirmId && x.CategoryId == firm.CategoryId && x.ProjectId == firm.ProjectId).DefaultIfEmpty()
                                       from categoryStatistics in q.For<Facts::ProjectCategoryStatistics>().Where(x => x.CategoryId == firm.CategoryId && x.ProjectId == firm.ProjectId).DefaultIfEmpty()
                                       from firmCount in firmCounts.Where(x => x.CategoryId == firm.CategoryId && x.ProjectId == firm.ProjectId).DefaultIfEmpty()
                                       select new CI::FirmCategoryStatistics
                                       {
                                           ProjectId = firm.ProjectId,
                                           FirmId = firm.FirmId,
                                           CategoryId = firm.CategoryId,
                                           Hits = firmStatistics.Hits,
                                           Shows = firmStatistics.Shows,
                                           FirmCount = firmCount.Count,
                                           AdvertisersShare = categoryStatistics.AdvertisersCount / firmCount.Count
                                       };
                            });
                }
            }
        }
    }
}