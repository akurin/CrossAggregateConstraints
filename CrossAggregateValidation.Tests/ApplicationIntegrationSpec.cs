using System;
using System.Linq;
using CrossAggregateValidation.Adapters.Persistance;
using CrossAggregateValidation.Adapters.Persistance.Constraints;
using CrossAggregateValidation.Adapters.Persistance.EventHandling;
using CrossAggregateValidation.Adapters.Persistance.EventHandling.SubscriptionStarting;
using CrossAggregateValidation.Adapters.Persistance.JsonNetEventSerialization;
using CrossAggregateValidation.Adapters.Persistance.Repositories;
using CrossAggregateValidation.Application.EventHandling;
using CrossAggregateValidation.Application.UserRegistration;
using CrossAggregateValidation.Tests.Adapters;
using CrossAggregateValidation.Tests.Adapters.Persistance;
using CrossAggregateValidation.Tests.Domain;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Embedded;
using EventStore.ClientAPI.SystemData;
using EventStore.Core;
using NSpec;
using Optional;

namespace CrossAggregateValidation.Tests
{
    public class ApplicationIntegrationSpec : nspec
    {
        private ClusterVNode _node;
        private IEventStoreConnection _connection;
        private UserRegistrationCommandService _commandService;
        private UserRegistrationQueryService _queryService;
        private EventStoreAllCatchUpSubscription _subscription;

        private void before_each()
        {
            var node = EmbeddedEventStore.StartAndWaitUntilReady();

            var connectionSettings = ConnectionSettings
                .Create()
                .SetDefaultUserCredentials(new UserCredentials("admin", "changeit"));

            var embeddedConnection = EmbeddedEventStoreConnection.Create(node, connectionSettings);
            embeddedConnection.ConnectAsync().Wait();

            var eventSerializer = JsonNetEventSerializer.CreateForAssembly(
                typeof(CrossAggregateValidation.Domain.IEvent).Assembly);

            var userRegistrationProcessRepository = new UserRegistrationProcessRepository(embeddedConnection, eventSerializer);
            var userByEmailInMemoryIndex = new UserByEmailIndex(embeddedConnection, eventSerializer);

            var commandService = new UserRegistrationCommandService(userRegistrationProcessRepository);
            var queryService = new UserRegistrationQueryService(userRegistrationProcessRepository);

            var userRepository = new UserRepository(embeddedConnection, eventSerializer);

            var userRegistrationEventHandler = new UserRegistrationEventHandler(
                userRegistrationProcessRepository,
                userRepository,
                userByEmailInMemoryIndex);

            var userRegistrationEventHandlerAdapter = new UserRegistrationEventHandlerAdapter(userRegistrationEventHandler);
            var subscription = new SubscriptionStarter()
                .WithConnection(embeddedConnection)
                .WithPositionStorage(new EventStorePositionStorage(embeddedConnection, "position", new JsonPositionSerializer()))
                .WithEventSerializer(eventSerializer)
                .WithEventHandler(userRegistrationEventHandlerAdapter)
                .IgnoreSubscriptionDrop()
                .StartAsync().Result;

            _node = node;
            _connection = embeddedConnection;
            _commandService = commandService;
            _queryService = queryService;
            _subscription = subscription;
        }

        private void after_each()
        {
            _subscription?.Stop();
            _connection?.Close();
            _node?.Stop();
        }

        private void describe_application()
        {
            context["when two user registration processes have been started for users with same emails"] = () =>
            {
                var userId1 = new Guid();
                var userId2 = new Guid();

                before = () =>
                {
                    var userRegistrationForm = UserRegistrationFormMother.JohnDow();

                    userId1 = _commandService
                        .StartRegistrationAsync(userRegistrationForm)
                        .Result;

                    userId2 = _commandService
                        .StartRegistrationAsync(userRegistrationForm)
                        .Result;
                };

                it["eventually returns succeeded result for one user and failed result for another user"] = () =>
                {
                    Eventually.IsTrue(() =>
                    {
                        var queryResults = new[]
                        {
                            _queryService.GetAsync(userId1).Result,
                            _queryService.GetAsync(userId2).Result
                        };

                        return
                            queryResults.Contains(Option.Some(UserRegstrationProcessQueryResult.Succeeded)) &&
                            queryResults.Contains(Option.Some(UserRegstrationProcessQueryResult.Failed));
                    });
                };
            };
        }
    }
}