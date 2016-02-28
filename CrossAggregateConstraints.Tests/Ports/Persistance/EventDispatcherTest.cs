using System;
using System.Linq;
using System.Threading.Tasks;
using CrossAggregateConstraints.Infrastructure.EventSourcing;
using CrossAggregateConstraints.Ports.Persistance;
using CrossAggregateConstraints.Ports.Persistance.EventDispatching;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Embedded;
using EventStore.Core;
using NSpec;
using Optional;

namespace CrossAggregateConstraints.Tests.Ports.Persistance
{
    public class EventDispatcherTest : nspec
    {
        private IEventStoreConnection _connection;

        private ClusterVNode _node;
        private EventSerializerStub _serializer;
        private EventDispatcher _sut;
        private EventHandlerMock _eventHandlerMock;
        private EventDispatcherSuscription _subscription;

        private void before_each()
        {
            _node = TestEventStore.StartEmbedded();
            _connection = EmbeddedEventStoreConnection.Create(_node);
            _connection.ConnectAsync().Wait();

            _serializer = new EventSerializerStub();

            _eventHandlerMock = new EventHandlerMock();
            _sut = new EventDispatcher(new EventSerializerStub(), _eventHandlerMock);
            _subscription = _sut.Start(_connection);
        }

        private void after_each()
        {
            _subscription?.Stop();
            _connection?.Close();
            _node?.Stop();
        }

        private void describe_A()
        {
            context["when event has been added to EventStore"] = () =>
            {
                before = () =>
                {
                    var someEvent = new DummyEvent();
                    var eventData = _serializer.ToEventData(someEvent);
                    _connection.AppendToStreamAsync("dummy", ExpectedVersion.Any, eventData).Wait();
                };

                it["eventually calls event handler"] = () =>
                {
                    Eventually.IsTrue(() => _eventHandlerMock.WasCalledWithDummyEvent);
                };
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

                return Task.FromResult(0);
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