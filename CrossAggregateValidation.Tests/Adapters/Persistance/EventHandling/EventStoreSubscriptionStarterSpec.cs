using System;
using System.Linq;
using System.Threading.Tasks;
using CrossAggregateValidation.Adapters.Persistance;
using CrossAggregateValidation.Adapters.Persistance.EventHandling;
using CrossAggregateValidation.Domain;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Embedded;
using EventStore.Core;
using NSpec;
using Optional;

namespace CrossAggregateValidation.Tests.Adapters.Persistance.EventHandling
{
    public class EventStoreSubscriptionStarterSpec : nspec
    {
        private ClusterVNode _node;
        private IEventStoreConnection _connection;
        private EventSerializerStub _serializer;
        private EventHandlerMock _eventHandler;
        private EventStoreSubscriptionStarter _sut;
        private EventHandlingSubscription _subscription;

        private void before_each()
        {
            var embeddedNode = EmbeddedEventStore.Start();
            var embeddedConnection = EmbeddedEventStoreConnection.Create(embeddedNode);
            embeddedConnection.ConnectAsync().Wait();

            var eventSerializerStub = new EventSerializerStub();

            var eventHandlerMock = new EventHandlerMock();
            var sut = new EventStoreSubscriptionStarter(new EventSerializerStub(), eventHandlerMock);

            _node = embeddedNode;
            _connection = embeddedConnection;
            _serializer = eventSerializerStub;
            _eventHandler = eventHandlerMock;
            _sut = sut;
        }

        private void after_each()
        {
            _subscription?.Stop();
            _connection?.Close();
            _node?.Stop();
        }

        private void describe_Start()
        {
            context["when event has been added to EventStore"] = () =>
            {
                before = () =>
                {
                    var someEvent = new DummyEvent();
                    var eventData = _serializer.ToEventData(someEvent);
                    _connection.AppendToStreamAsync("dummy", ExpectedVersion.Any, eventData).Wait();

                    _subscription = _sut.Start(_connection);
                };

                after = () => _subscription?.Stop();

                it["eventually calls event handler"] =
                    () => Eventually.IsTrue(() => _eventHandler.WasCalledWithDummyEvent);
            };
        }

        private class DummyEvent : IEvent
        {
        }

        private class EventHandlerMock : IEventHandler
        {
            private volatile bool _wasCalledWithDummyEvent;

            public bool WasCalledWithDummyEvent
            {
                get { return _wasCalledWithDummyEvent; }
                private set { _wasCalledWithDummyEvent = value; }
            }

            public Task HandleAsync(IEvent @event)
            {
                if (@event is DummyEvent)
                {
                    WasCalledWithDummyEvent = true;
                }

                return Task.FromResult<object>(null);
            }
        }

        private class EventSerializerStub : IEventSerializer
        {
            public EventData ToEventData(IEvent @event)
            {
                return new EventData(
                    eventId: Guid.NewGuid(),
                    type: "DummyEventOccurred",
                    isJson: false,
                    data: new byte[] {1},
                    metadata: new byte[0]);
            }

            public Option<IEvent> FromEventData(RecordedEvent recordedEvent)
            {
                if (recordedEvent.EventType == "DummyEventOccurred" &&
                    recordedEvent.Data.SequenceEqual(new byte[] {1}))
                {
                    return Option.Some<IEvent>(new DummyEvent());
                }

                return Option.None<IEvent>();
            }
        }
    }
}