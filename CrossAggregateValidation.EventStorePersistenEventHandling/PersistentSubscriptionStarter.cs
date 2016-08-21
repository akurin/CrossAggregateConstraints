using System;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using Optional;

namespace ESUtils.PersistentSubscription
{
    public class PersistentSubscriptionStarter
    {
        private IEventStoreConnection _connection;
        private IPositionStorage _positionStorage;
        private Func<ResolvedEvent, Task> _handleEventAsync;
        private Action<SubscriptionDropReason, Option<Exception>> _handleSubscriptionDrop;

        public SetPositionStorageStep WithConnection(IEventStoreConnection connection)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));

            _connection = connection;
            return new SetPositionStorageStep(this);
        }

        public class SetPositionStorageStep
        {
            private readonly PersistentSubscriptionStarter _starter;

            internal SetPositionStorageStep(PersistentSubscriptionStarter starter)
            {
                if (starter == null) throw new ArgumentNullException(nameof(starter));

                _starter = starter;
            }

            public SetEventHandlerStep WithPositionStorage(IPositionStorage positionStorage)
            {
                if (positionStorage == null) throw new ArgumentNullException(nameof(positionStorage));

                _starter._positionStorage = positionStorage;
                return new SetEventHandlerStep(_starter);
            }
        }

        public class SetEventHandlerStep
        {
            private readonly PersistentSubscriptionStarter _starter;

            internal SetEventHandlerStep(PersistentSubscriptionStarter starter)
            {
                if (starter == null) throw new ArgumentNullException(nameof(starter));

                _starter = starter;
            }

            public SetSubscriptionDropHandlerStep WithEventHandler(Func<ResolvedEvent, Task> handleEventAsync)
            {
                if (handleEventAsync == null) throw new ArgumentNullException(nameof(handleEventAsync));

                _starter._handleEventAsync = handleEventAsync;
                return new SetSubscriptionDropHandlerStep(_starter);
            }
        }

        public class SetSubscriptionDropHandlerStep
        {
            private readonly PersistentSubscriptionStarter _starter;

            internal SetSubscriptionDropHandlerStep(PersistentSubscriptionStarter starter)
            {
                if (starter == null) throw new ArgumentNullException(nameof(starter));

                _starter = starter;
            }

            public BuildStep WithSubscriptionDropHandler(
                Action<SubscriptionDropReason, Option<Exception>> handleSubscriptionDrop)
            {
                if (handleSubscriptionDrop == null) throw new ArgumentNullException(nameof(handleSubscriptionDrop));

                _starter._handleSubscriptionDrop = handleSubscriptionDrop;
                return new BuildStep(_starter);
            }

            public BuildStep IgnoreSubscriptionDrop()
            {
                return WithSubscriptionDropHandler((reason, maybeException) => { });
            }
        }

        public class BuildStep
        {
            private readonly PersistentSubscriptionStarter _starter;

            public BuildStep(PersistentSubscriptionStarter starter)
            {
                _starter = starter;
            }

            public Task<EventStoreAllCatchUpSubscription> StartAsync()
            {
                return _starter.StartAsync();
            }
        }

        private async Task<EventStoreAllCatchUpSubscription> StartAsync()
        {
            var messageQueue = new AwaitableQueue();

            var lastCheckpoint = await ReadLastCheckpointAsync();

            var subscription = _connection.SubscribeToAllFrom(
                lastCheckpoint: lastCheckpoint,
                settings: CatchUpSubscriptionSettings.Default,
                eventAppeared: (s, resolvedEvent) => messageQueue.Send(resolvedEvent),
                subscriptionDropped: (s, reason, ex) =>
                    messageQueue.Send(new SubscriptionDropped(reason, ex)));

            var consumer = new AsyncEventHandlerCaller(
                messageQueue: messageQueue,
                positionStorage: _positionStorage,
                handleEventAsync: _handleEventAsync,
                handleSubscriptionDrop: (reason, ex) =>
                    _handleSubscriptionDrop(reason, ex?.Some() ?? Option.None<Exception>()));

            StartConsumerAsync(consumer, messageQueue, subscription);

            return subscription;
        }

        private async Task<Position?> ReadLastCheckpointAsync()
        {
            var maybePosition = await _positionStorage.ReadAsync();
            return maybePosition.Match(
                position => position,
                () => AllCheckpoint.AllStart);
        }

        private static async void StartConsumerAsync(
            AsyncEventHandlerCaller eventHandlerCaller,
            AwaitableQueue messageQueue,
            EventStoreCatchUpSubscription subscription)
        {
            try
            {
                await eventHandlerCaller.WorkAsync();
            }
            catch (Exception exception)
            {
                messageQueue.Send(
                    new SubscriptionDropped(SubscriptionDropReason.EventHandlerException, exception));

                subscription.Stop();
            }
        }
    }
}