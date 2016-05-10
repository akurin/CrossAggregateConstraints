using System;
using System.Threading.Tasks;
using CrossAggregateValidation.Domain;
using EventStore.ClientAPI;
using Optional;

namespace CrossAggregateValidation.Adapters.Persistance.EventHandling.SubscriptionStarting
{
    public class SubscriptionStarter
    {
        private IEventStoreConnection _connection;
        private IPositionStorage _positionStorage;
        private IEventSerializer _eventSerializer;
        private Func<IEvent, Task> _handleEventAsync;
        private Action<SubscriptionDropReason, Option<Exception>> _handleSubscriptionDrop;

        public SetPositionStorageStep WithConnection(IEventStoreConnection connection)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));

            _connection = connection;
            return new SetPositionStorageStep(this);
        }

        public class SetPositionStorageStep
        {
            private readonly SubscriptionStarter _starter;

            internal SetPositionStorageStep(SubscriptionStarter starter)
            {
                if (starter == null) throw new ArgumentNullException(nameof(starter));

                _starter = starter;
            }

            public SetEventSerializerStep WithPositionStorage(IPositionStorage positionStorage)
            {
                if (positionStorage == null) throw new ArgumentNullException(nameof(positionStorage));

                _starter._positionStorage = positionStorage;
                return new SetEventSerializerStep(_starter);
            }
        }

        public class SetEventSerializerStep
        {
            private readonly SubscriptionStarter _starter;

            internal SetEventSerializerStep(SubscriptionStarter starter)
            {
                if (starter == null) throw new ArgumentNullException(nameof(starter));

                _starter = starter;
            }

            public SetEventHandlerStep WithEventSerializer(IEventSerializer eventSerializer)
            {
                if (eventSerializer == null) throw new ArgumentNullException(nameof(eventSerializer));

                _starter._eventSerializer = eventSerializer;
                return new SetEventHandlerStep(_starter);
            }
        }

        public class SetEventHandlerStep
        {
            private readonly SubscriptionStarter _starter;

            internal SetEventHandlerStep(SubscriptionStarter starter)
            {
                if (starter == null) throw new ArgumentNullException(nameof(starter));

                _starter = starter;
            }

            public SetSubscriptionDropHandlerStep WithEventHandler(Func<IEvent, Task> handleEventAsync)
            {
                if (handleEventAsync == null) throw new ArgumentNullException(nameof(handleEventAsync));

                _starter._handleEventAsync = handleEventAsync;
                return new SetSubscriptionDropHandlerStep(_starter);
            }

            public SetSubscriptionDropHandlerStep WithEventHandler(IEventHandler eventHandler)
            {
                if (eventHandler == null) throw new ArgumentNullException(nameof(eventHandler));

                return WithEventHandler(eventHandler.HandleAsync);
            }
        }

        public class SetSubscriptionDropHandlerStep
        {
            private readonly SubscriptionStarter _starter;

            internal SetSubscriptionDropHandlerStep(SubscriptionStarter starter)
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
            private readonly SubscriptionStarter _starter;

            public BuildStep(SubscriptionStarter starter)
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
            var appearedEventHandler = new AppearedEventHandler(_eventSerializer, messageQueue);

            var lastCheckpoint = await ReadLastCheckpointAsync();

            var subscription = _connection.SubscribeToAllFrom(
                lastCheckpoint: lastCheckpoint,
                settings: CatchUpSubscriptionSettings.Default,
                eventAppeared: appearedEventHandler.Handle,
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