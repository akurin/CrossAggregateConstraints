using System.Threading.Tasks;
using CrossAggregateConstraints.Application;
using CrossAggregateConstraints.Domain;
using CrossAggregateConstraints.Ports.Costraints;
using CrossAggregateConstraints.Ports.Persistance;
using CrossAggregateConstraints.Ports.Persistance.EventDispatching;
using CrossAggregateConstraints.Ports.Persistance.Repositories;
using CrossAggregateConstraints.Tests.Ports;
using CrossAggregateConstraints.Tests.Ports.Persistance;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Embedded;
using EventStore.ClientAPI.SystemData;
using NUnit.Framework;
using Optional.Unsafe;

namespace CrossAggregateConstraints.Tests
{
    [TestFixture]
    public class AcceptanceTests
    {
        [Test]
        public async Task Test()
        {
            var node = TestEventStore.StartEmbedded();

            var settings = ConnectionSettings
                .Create()
                .SetDefaultUserCredentials(new UserCredentials("admin", "changeit"));

            var connection = EmbeddedEventStoreConnection.Create(node, settings);
            await connection.ConnectAsync();

            var crossAggregateConstraints = new InMemoryCrossAggregateConstraints();

            var userRegistrationProcessFactory = new UserRegistrationProcessFactory(crossAggregateConstraints);

            var eventSerializer = new EventSerializer();
            var userRegistrationProcessRepository = new UserRegistrationProcessRepository(connection, eventSerializer, userRegistrationProcessFactory);
            var userRepository = new UserRepository(connection, eventSerializer);

            var userRegistrationProcessExecutive = new UserRegistrationProcessManager(
                userRegistrationProcessFactory,
                userRegistrationProcessRepository,
                userRepository);

            var eventHandler = new UserRegistrtionProcessManagerEventHandler(userRegistrationProcessExecutive);
            var eventDispatcher = new EventDispatcher(eventSerializer, eventHandler);
            var subscription = eventDispatcher.Start(connection);

            var userRegistrationForm = new UserRegistrationForm("a@example.com");

            var userId = await userRegistrationProcessExecutive.StartRegistrationAsync(userRegistrationForm);

            await Eventually.IsTrueAsync(async () =>
            {
                var maybeProcess = await userRegistrationProcessRepository.GetAsync(userId);
                return maybeProcess.ValueOrFailure().State == UserRegistrationProcessState.Succeeded;
            });

            subscription.Stop();
            connection.Close();
            node.Stop();
        }
    }
}