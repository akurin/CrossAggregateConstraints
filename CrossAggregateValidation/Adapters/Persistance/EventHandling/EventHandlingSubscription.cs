using System;
using System.Threading;
using EventStore.ClientAPI;

namespace CrossAggregateValidation.Adapters.Persistance.EventHandling
{
    public class EventHandlingSubscription
    {
        private readonly EventStoreCatchUpSubscription _subscription;
        private readonly CancellationTokenSource _cts;

        public EventHandlingSubscription(EventStoreCatchUpSubscription subscription, CancellationTokenSource cts)
        {
            if (subscription == null) throw new ArgumentNullException(nameof(subscription));
            if (cts == null) throw new ArgumentNullException(nameof(cts));

            _subscription = subscription;
            _cts = cts;
        }

        public void Stop()
        {
            _subscription.Stop();
            _cts.Cancel();
        }
    }
}