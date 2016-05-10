using System;
using EventStore.ClientAPI;

namespace CrossAggregateValidation.Adapters.Persistance.EventHandling.SubscriptionStarting
{
    internal class SubscriptionDropped
    {
        public SubscriptionDropped(SubscriptionDropReason dropReason, Exception exception)
        {
            DropReason = dropReason;
            Exception = exception;
        }

        public SubscriptionDropReason DropReason { get; }
        public Exception Exception { get; }
    }
}