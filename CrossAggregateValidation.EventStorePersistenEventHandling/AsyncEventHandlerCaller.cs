using System;
using System.Threading.Tasks;
using EventStore.ClientAPI;

namespace ESUtils.PersistentSubscription
{
    internal class AsyncEventHandlerCaller
    {
        private readonly AwaitableQueue _messageQueue;
        private readonly Func<ResolvedEvent, Task> _handleEventAsync;
        private readonly IPositionStorage _positionStorage;
        private readonly Action<SubscriptionDropReason, Exception> _handleSubscriptionDrop;
        private bool _stopped;

        public AsyncEventHandlerCaller(
            AwaitableQueue messageQueue,
            IPositionStorage positionStorage,
            Func<ResolvedEvent, Task> handleEventAsync,
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
                if (message is ResolvedEvent)
                {
                    var resolvedEvent = (ResolvedEvent) message;
                    var originalPosition = resolvedEvent.OriginalPosition;
                    if (originalPosition == null)
                        continue;

                    await _handleEventAsync(resolvedEvent);
                    await _positionStorage.WriteAsync(originalPosition.Value);
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