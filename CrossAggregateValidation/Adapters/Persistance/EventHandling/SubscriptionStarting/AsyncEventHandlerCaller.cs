using System;
using System.Threading.Tasks;
using CrossAggregateValidation.Domain;
using EventStore.ClientAPI;

namespace CrossAggregateValidation.Adapters.Persistance.EventHandling.SubscriptionStarting
{
    internal class AsyncEventHandlerCaller
    {
        private readonly AwaitableQueue _messageQueue;
        private readonly Func<IEvent, Task> _handleEventAsync;
        private readonly IPositionStorage _positionStorage;
        private readonly Action<SubscriptionDropReason, Exception> _handleSubscriptionDrop;
        private bool _stopped;

        public AsyncEventHandlerCaller(
            AwaitableQueue messageQueue,
            IPositionStorage positionStorage,
            Func<IEvent, Task> handleEventAsync,
            Action<SubscriptionDropReason, Exception> handleSubscriptionDrop)
        {
            _messageQueue = messageQueue;
            _handleEventAsync = handleEventAsync;
            _positionStorage = positionStorage;
            _handleSubscriptionDrop = handleSubscriptionDrop;
        }

        public async Task WorkAsync()
        {
            while (!_stopped)
            {
                var message = await _messageQueue.TakeAsync();
                if (message is EventAppeared)
                {
                    var eventAppeared = message as EventAppeared;
                    await _handleEventAsync(eventAppeared.Event);
                    await _positionStorage.WriteAsync(eventAppeared.Position);
                }
                else if (message is SubscriptionDropped)
                {
                    var subscriptionDropped = message as SubscriptionDropped;
                    _stopped = true;
                    _handleSubscriptionDrop(subscriptionDropped.DropReason, subscriptionDropped.Exception);
                }
                else
                    throw new InvalidOperationException($"Unexpected message {message.GetType()}");
            }
        }
    }
}