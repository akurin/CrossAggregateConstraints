using System;
using EventStore.ClientAPI;

namespace CrossAggregateValidation.Adapters.Persistance.EventHandling.SubscriptionStarting
{
    internal class AppearedEventHandler
    {
        private readonly IEventSerializer _eventSerializer;
        private readonly AwaitableQueue _messageQueue;

        public AppearedEventHandler(
            IEventSerializer eventSerializer,
            AwaitableQueue messageQueue)
        {
            if (eventSerializer == null) throw new ArgumentNullException(nameof(eventSerializer));
            if (messageQueue == null) throw new ArgumentNullException(nameof(messageQueue));

            _eventSerializer = eventSerializer;
            _messageQueue = messageQueue;
        }

        public void Handle(EventStoreCatchUpSubscription subscription, ResolvedEvent resolvedEvent)
        {
            var originalPosition = resolvedEvent.OriginalPosition;
            if (originalPosition == null)
                return;

            var maybeEvent = _eventSerializer.FromEventData(resolvedEvent.OriginalEvent);
            maybeEvent.Match(
                e => _messageQueue.Send(new EventAppeared(originalPosition.Value, e)),
                () => { });
        }
    }
}