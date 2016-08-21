using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CrossAggregateValidation.Domain;
using ESUtils.PersistentSubscription;
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
                var someEvent = CreateEventOfType("someType");

                before = () =>
                {
                    subscription = new PersistentSubscriptionStarter()
                        .WithConnection(_connection)
                        .WithPositionStorage(CreateEventStorePositionStorage())
                        .WithEventHandler(eventHandler.HandleAsync)
                        .IgnoreSubscriptionDrop()
                        .StartAsync().Result;

                    _connection.AppendToStreamAsync("someStream", ExpectedVersion.Any, someEvent).Wait();
                };

                after = () => subscription?.Stop();

                it["eventually calls event handler"] =
                    () => Eventually.IsTrue(() =>
                    {
                        var userDefinedEvents = eventHandler.HandledUserDefinedEvents();
                        return
                            userDefinedEvents.Any() &&
                            userDefinedEvents.First().Event.EventType == someEvent.Type;
                    });
            };

            context["event stream contains not handled events"] = () =>
            {
                var subscription = default(EventStoreAllCatchUpSubscription);
                var eventHandler = new EventHandlerSpy();
                var firstEvent = CreateEventOfType("firstEventType");
                var secondEvent = CreateEventOfType("secondEventType");

                before = () =>
                {
                    AddEventsToSomeStream(firstEvent, secondEvent);
                    WaitForHandlerThatFailsIfEventTypeIsEqualTo(secondEvent.Type);

                    subscription = new PersistentSubscriptionStarter()
                        .WithConnection(_connection)
                        .WithPositionStorage(CreateEventStorePositionStorage())
                        .WithEventHandler(eventHandler.HandleAsync)
                        .IgnoreSubscriptionDrop()
                        .StartAsync().Result;
                };

                it["eventually calls event handler for not handled events"] = () =>
                {
                    Eventually.IsTrue(() =>
                    {
                        var eventType = eventHandler.HandledUserDefinedEvents()
                            .Select(e => e.Event.EventType)
                            .FirstOrDefault();

                        return eventType == secondEvent.Type;
                    });
                };

                after = () => subscription?.Stop();
            };
        }

        private void WaitForHandlerThatFailsIfEventTypeIsEqualTo(string eventType)
        {
            var tcs = new TaskCompletionSource<object>();

            EventStoreAllCatchUpSubscription subscription = null;
            try
            {
                subscription = new PersistentSubscriptionStarter()
                    .WithConnection(_connection)
                    .WithPositionStorage(CreateEventStorePositionStorage())
                    .WithEventHandler(e =>
                    {
                        if (e.Event.EventType == eventType)
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

        private void AddEventsToSomeStream(params EventData[] eventsData)
        {
            _connection.AppendToStreamAsync("someStream", ExpectedVersion.Any, eventsData).Wait();
        }

        private static EventData CreateEventOfType(string type)
        {
            return new EventData(
                eventId: Guid.NewGuid(),
                type: type,
                isJson: false,
                data: new byte[0],
                metadata: new byte[0]);
        }

        private class EventHandlerSpy
        {
            private readonly object _lockObject = new object();
            private readonly List<ResolvedEvent> _events = new List<ResolvedEvent>();

            public Task HandleAsync(ResolvedEvent resolvedEvent)
            {
                lock (_lockObject)
                {
                    _events.Add(resolvedEvent);
                }

                return Task.FromResult<object>(null);
            }

            public IEnumerable<ResolvedEvent> HandledUserDefinedEvents()
            {
                lock (_lockObject)
                {
                    return _events.Where(IsUserDefinedEvent).ToList();
                }
            }

            private static bool IsUserDefinedEvent(ResolvedEvent resolvedEvent)
            {
                return !resolvedEvent.Event.EventType.StartsWith("$");
            }
        }
    }
}