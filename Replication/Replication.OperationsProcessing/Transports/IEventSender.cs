﻿using System.Collections.Generic;

using NuClear.Messaging.API.Flows;
using NuClear.River.Common.Metadata;

namespace NuClear.Replication.OperationsProcessing.Transports
{
    public interface IEventSender
    {
        void Push<TEvent, TFlow>(TFlow targetFlow, IReadOnlyCollection<TEvent> events)
            where TFlow : IMessageFlow
            where TEvent : IEvent;
    }
}