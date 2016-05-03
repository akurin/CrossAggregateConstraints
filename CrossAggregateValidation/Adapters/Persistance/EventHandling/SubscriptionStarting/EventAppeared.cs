using System;
using CrossAggregateValidation.Domain;
using EventStore.ClientAPI;

namespace CrossAggregateValidation.Adapters.Persistance.EventHandling.SubscriptionStarting
{
    internal class EventAppeared : IMessage
    {
        public EventAppeared(Position position, IEvent @event)
        {
            if (@event == null) throw new ArgumentNullException(nameof(@event));

            Position = position;
            Event = @event;
        }

        public Position Position { get; }
        public IEvent Event { get; }
    }
}