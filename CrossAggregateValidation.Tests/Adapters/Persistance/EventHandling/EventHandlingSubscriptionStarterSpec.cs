using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CrossAggregateValidation.Adapters.Persistance;
using CrossAggregateValidation.Adapters.Persistance.EventHandling;
using CrossAggregateValidation.Adapters.Persistance.EventHandling.SubscriptionStarting;
using CrossAggregateValidation.Adapters.Persistance.JsonNetEventSerialization;
using CrossAggregateValidation.Domain;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Embedded;
using EventStore.ClientAPI.SystemData;
using EventStore.Core;
using NSpec;

namespace CrossAggregateValidation.Tests.Adapters.Persistance.EventHandling
{
    public class EventHandlingSubscriptionStarterSpec : nspec
    {
        private ClusterVNode _node;
        private IEventStoreConnection _connection;

        private void before_each()
        {
            var embeddedNode = EmbeddedEventStore.StartAndWaitUntilReady();
            var connectionSettings = ConnectionSettings
                .Create()
                .SetDefaultUserCredentials(new UserCredentials("admin", "changeit"));

            var embeddedConnection = EmbeddedEventStoreConnection.Create(embeddedNode, connectionSettings);
            embeddedConnection.ConnectAsync().Wait();

            _node = embeddedNode;
            _connection = embeddedConnection;
        }

        private void after_each()
        {
            _connection?.Close();
            _node?.Stop();
        }

        private void describe_StartAsync()
        {
            context["when event has been added to EventStore"] = () =>
            {
                var subscription = default(EventStoreAllCatchUpSubscription);
                var eventHandler = new EventHandlerSpy();
                var someEvent = SomeEvent.New();

                before = () =>
                {
                    subscription = new SubscriptionStarter()
                        .WithConnection(_connection)
                        .WithPositionStorage(CreateEventStorePositionStorage())
                        .WithEventSerializer(CreateEventSerializer())
                        .WithEventHandler(eventHandler)
                        .IgnoreSubscriptionDrop()
                        .StartAsync().Result;

                    var eventData = CreateEventSerializer().ToEventData(someEvent);
                    _connection.AppendToStreamAsync("someStream", ExpectedVersion.Any, eventData).Wait();
                };

                after = () => subscription?.Stop();

                it["eventually calls event handler"] =
                    () => Eventually.IsTrue(() =>
                    {
                        var firstHandledEvent = eventHandler.HandledEvents.FirstOrDefault();
                        return someEvent.Equals(firstHandledEvent);
                    });
            };

            context["event stream contains not handled events"] = () =>
            {
                var subscription = default(EventStoreAllCatchUpSubscription);
                var eventHandler = new EventHandlerSpy();
                var firstEvent = SomeEvent.New();
                var secondEvent = SomeEvent.New();

                before = () =>
                {
                    AddEventsToSomeStream(firstEvent, secondEvent);
                    WaitForHandlerThatFailsIfEventIsNotEqual(firstEvent);

                    subscription = new SubscriptionStarter()
                        .WithConnection(_connection)
                        .WithPositionStorage(CreateEventStorePositionStorage())
                        .WithEventSerializer(CreateEventSerializer())
                        .WithEventHandler(eventHandler)
                        .IgnoreSubscriptionDrop()
                        .StartAsync().Result;
                };

                it["eventually calls event handler for not handled events"] = () =>
                {
                    Eventually.IsTrue(() =>
                    {
                        var firstHandledEvent = eventHandler.HandledEvents.FirstOrDefault();
                        return secondEvent.Equals(firstHandledEvent);
                    });
                };

                after = () => subscription?.Stop();
            };
        }

        private static IEventSerializer CreateEventSerializer()
        {
            return JsonNetEventSerializer.CreateForAssembly(typeof(EventHandlingSubscriptionStarterSpec).Assembly);
        }

        private void WaitForHandlerThatFailsIfEventIsNotEqual(IEvent @event)
        {
            var tcs = new TaskCompletionSource<object>();

            EventStoreAllCatchUpSubscription subscription = null;
            try
            {
                subscription = new SubscriptionStarter()
                    .WithConnection(_connection)
                    .WithPositionStorage(CreateEventStorePositionStorage())
                    .WithEventSerializer(CreateEventSerializer())
                    .WithEventHandler(e =>
                    {
                        if (!e.Equals(@event))
                        {
                            tcs.SetResult(null);
                            throw new Exception("let's stop subscription!");
                        }

                        return Task.FromResult<object>(null);
                    })
                    .IgnoreSubscriptionDrop()
                    .StartAsync().Result;
            }
            finally
            {
                subscription?.Stop();
            }

            if (!tcs.Task.Wait(TimeSpan.FromSeconds(3)))
                throw new Exception("Event has not been handled in time");
        }

        private EventStorePositionStorage CreateEventStorePositionStorage()
        {
            return new EventStorePositionStorage(_connection, "position", new JsonPositionSerializer());
        }

        private void AddEventsToSomeStream(params IEvent[] events)
        {
            foreach (var @event in events)
            {
                var eventData = CreateEventSerializer().ToEventData(@event);
                _connection.AppendToStreamAsync("someStream", ExpectedVersion.Any, eventData).Wait();
            }
        }

        public class SomeEvent : IEvent
        {
            public static SomeEvent New()
            {
                return new SomeEvent(Guid.NewGuid());
            }

            public SomeEvent(Guid id)
            {
                Id = id;
            }

            public Guid Id { get; private set; }

            private bool Equals(SomeEvent other)
            {
                return Id.Equals(other.Id);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((SomeEvent) obj);
            }

            public override int GetHashCode()
            {
                return Id.GetHashCode();
            }
        }

        private class EventHandlerSpy : IEventHandler
        {
            private readonly object _lockObject = new object();
            private readonly List<IEvent> _events = new List<IEvent>();

            public Task HandleAsync(IEvent @event)
            {
                lock (_lockObject)
                {
                    _events.Add(@event);
                }

                return Task.FromResult<object>(null);
            }

            public IEnumerable<IEvent> HandledEvents
            {
                get
                {
                    lock (_lockObject)
                    {
                        return _events.ToList();
                    }
                }
            }
        }
    }
}