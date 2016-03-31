using System;
using System.Linq;
using CrossAggregateConstraints.Application.EventHandling;
using CrossAggregateConstraints.Application.UserRegistration;
using CrossAggregateConstraints.Ports.Constraints;
using CrossAggregateConstraints.Ports.Persistance;
using CrossAggregateConstraints.Ports.Persistance.EventHandling;
using CrossAggregateConstraints.Ports.Persistance.Repositories;
using CrossAggregateConstraints.Tests.Adapters;
using CrossAggregateConstraints.Tests.Adapters.Persistance;
using CrossAggregateConstraints.Tests.Domain;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Embedded;
using EventStore.ClientAPI.SystemData;
using EventStore.Core;
using NSpec;
using Optional;

namespace CrossAggregateConstraints.Tests
{
    public class ApplicationIntegrationSpec : nspec
    {
        private ClusterVNode _node;
        private IEventStoreConnection _connection;
        private UserRegistrationCommandService _commandService;
        private UserRegistrationQueryService _queryService;
        private CrossAggregateConstraints.Ports.Persistance.EventHandling.EventStoreSubscription _subscription;

        private void before_each()
        {
            var node = EmbeddedEventStore.Start();

            var connectionSettings = ConnectionSettings
                .Create()
                .SetDefaultUserCredentials(new UserCredentials("admin", "changeit"));

            var connection = EmbeddedEventStoreConnection.Create(node, connectionSettings);
            connection.ConnectAsync().Wait();

            var inMemoryUserByEmailIndex = new UserByEmailInMemoryIndex();

            var eventSerializer = new EventSerializer();
            var userRegistrationProcessRepository = new UserRegistrationProcessRepository(connection, eventSerializer);

            var commandService = new UserRegistrationCommandService(userRegistrationProcessRepository);
            var queryService = new UserRegistrationQueryService(userRegistrationProcessRepository);

            var userRepository = new UserRepository(connection, eventSerializer);

            var userRegistrationEventHandler = new UserRegistrationEventHandler(
                userRegistrationProcessRepository,
                userRepository,
                inMemoryUserByEmailIndex);

            var userRegistrationEventHandlerAdapter = new UserRegistrationEventHandlerAdapter(userRegistrationEventHandler);
            var subscriptionStarter = new EventStoreSubscriptionStarter(eventSerializer, userRegistrationEventHandlerAdapter);
            var subscription = subscriptionStarter.Start(connection);

            _node = node;
            _connection = connection;
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