using System.Threading;
using EventStore.ClientAPI;

namespace CrossAggregateConstraints.Ports.Persistance.EventDispatching
{
    public sealed class EventDispatcherSuscription
    {
        private readonly EventStoreCatchUpSubscription _subscription;
        private readonly CancellationTokenSource _cts;

        public EventDispatcherSuscription(EventStoreCatchUpSubscription subscription, CancellationTokenSource cts)
        {
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