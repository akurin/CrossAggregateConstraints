using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using CrossAggregateConstraints.Domain;
using EventStore.ClientAPI;

namespace CrossAggregateConstraints.Ports.Persistance.EventHandling
{
    public class EventStoreSubscriptionStarter
    {
        private readonly IEventSerializer _eventSerializer;
        private readonly IEventHandler _eventHandler;
        private readonly BlockingCollection<IEvent> _blockingCollection = new BlockingCollection<IEvent>(); 

        public EventStoreSubscriptionStarter(IEventSerializer eventSerializer, IEventHandler eventHandler)
        {
            if (eventSerializer == null) throw new ArgumentNullException(nameof(eventSerializer));
            if (eventHandler == null) throw new ArgumentNullException(nameof(eventHandler));

            _eventSerializer = eventSerializer;
            _eventHandler = eventHandler;
        }

        public EventStoreHandlingSubscription Start(IEventStoreConnection connection)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));

            var cts = new CancellationTokenSource();
            Task.Run(() => HandleEvents(), cts.Token);

            var subscription = connection.SubscribeToAllFrom(
                lastCheckpoint: AllCheckpoint.AllStart,
                resolveLinkTos: true,
                eventAppeared: OnEventAppeared);

            return new EventStoreHandlingSubscription(subscription, cts);
        }

        private void HandleEvents()
        {
            foreach (var @event in _blockingCollection.GetConsumingEnumerable())
            {
                _eventHandler.HandleAsync(@event).Wait();
            }
        }

        private void OnEventAppeared(EventStoreCatchUpSubscription subscription, ResolvedEvent resolvedEvent)
        {
            var maybeEvent = _eventSerializer.FromEventData(resolvedEvent.Event);
            maybeEvent.Match(
                @event =>
                {
                    var originalPosition = resolvedEvent.OriginalPosition;
                    var eventHasBeenDeleted = originalPosition == null;
                    if (eventHasBeenDeleted)
                        return;

                    _blockingCollection.Add(@event);
                },
                () => { });
        }
    }
}