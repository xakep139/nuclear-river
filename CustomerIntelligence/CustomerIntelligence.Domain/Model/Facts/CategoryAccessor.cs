﻿using System.Collections.Generic;
using System.Linq;

using NuClear.CustomerIntelligence.Domain.Commands;
using NuClear.CustomerIntelligence.Domain.Events;
using NuClear.CustomerIntelligence.Domain.Specifications;
using NuClear.Replication.Core.API;
using NuClear.River.Common.Metadata;
using NuClear.Storage.API.Readings;
using NuClear.Storage.API.Specifications;

namespace NuClear.CustomerIntelligence.Domain.Model.Facts
{
    public sealed class CategoryAccessor : IStorageBasedDataObjectAccessor<Category>, IDataChangesHandler<Category>
    {
        private readonly IQuery _query;

        public CategoryAccessor(IQuery query)
        {
            _query = query;
        }

        public IQueryable<Category> GetSource() => Specs.Map.Erm.ToFacts.Categories.Map(_query);

        public FindSpecification<Category> GetFindSpecification(IReadOnlyCollection<ICommand> commands)
            => new FindSpecification<Category>(x => commands.Cast<SyncDataObjectCommand>().Select(c => c.DataObjectId).Contains(x.Id));

        public IReadOnlyCollection<IEvent> HandleCreates(IReadOnlyCollection<Category> dataObjects)
            => dataObjects.Select(x => new DataObjectCreatedEvent(typeof(Category), x.Id)).ToArray();

        public IReadOnlyCollection<IEvent> HandleUpdates(IReadOnlyCollection<Category> dataObjects)
            => dataObjects.Select(x => new DataObjectUpdatedEvent(typeof(Category), x.Id)).ToArray();

        public IReadOnlyCollection<IEvent> HandleDeletes(IReadOnlyCollection<Category> dataObjects)
            => dataObjects.Select(x => new DataObjectDeletedEvent(typeof(Category), x.Id)).ToArray();

        public IReadOnlyCollection<IEvent> HandleRelates(IReadOnlyCollection<Category> dataObjects)
        {
            var ids = dataObjects.Select(x => x.Id).ToArray();
            var specification = new FindSpecification<Account>(x => ids.Contains(x.Id));

            return Specs.Map.Facts.ToFirmAggregate.ByAccount(specification)
                        .Map(_query)
                        .Select(x => new RelatedDataObjectOutdatedEvent<long>(typeof(Firm), x))
                        .ToArray();
        }
    }
}