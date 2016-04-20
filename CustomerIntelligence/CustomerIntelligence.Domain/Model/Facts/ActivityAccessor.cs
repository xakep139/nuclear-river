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
    public sealed class ActivityAccessor : IStorageBasedDataObjectAccessor<Activity>, IDataChangesHandler<Activity>
    {
        private readonly IQuery _query;

        public ActivityAccessor(IQuery query)
        {
            _query = query;
        }

        public IQueryable<Activity> GetSource() => Specs.Map.Erm.ToFacts.Activities.Map(_query);

        public FindSpecification<Activity> GetFindSpecification(IReadOnlyCollection<ICommand> commands)
            => new FindSpecification<Activity>(x => commands.Cast<SyncDataObjectCommand>().Select(c => c.DataObjectId).Contains(x.Id));

        public IReadOnlyCollection<IEvent> HandleCreates(IReadOnlyCollection<Activity> dataObjects)
            => dataObjects.Select(x => new DataObjectCreatedEvent(typeof(Activity), x.Id)).ToArray();

        public IReadOnlyCollection<IEvent> HandleUpdates(IReadOnlyCollection<Activity> dataObjects)
            => dataObjects.Select(x => new DataObjectUpdatedEvent(typeof(Activity), x.Id)).ToArray();

        public IReadOnlyCollection<IEvent> HandleDeletes(IReadOnlyCollection<Activity> dataObjects)
            => dataObjects.Select(x => new DataObjectDeletedEvent(typeof(Activity), x.Id)).ToArray();

        public IReadOnlyCollection<IEvent> HandleRelates(IReadOnlyCollection<Activity> dataObjects)
        {
            var ids = dataObjects.Select(x => x.Id).ToArray();
            var specification = new FindSpecification<Activity>(x => ids.Contains(x.Id));

            IEnumerable<IEvent> events = Specs.Map.Facts.ToFirmAggregate.ByActivity(specification)
                                              .Map(_query)
                                              .Select(x => new RelatedDataObjectOutdatedEvent<long>(typeof(Firm), x));

            events = events.Concat(Specs.Map.Facts.ToFirmAggregate.ByClientActivity(specification)
                                        .Map(_query)
                                        .Select(x => new RelatedDataObjectOutdatedEvent<long>(typeof(Firm), x)));
            return events.ToArray();
        }
    }
}