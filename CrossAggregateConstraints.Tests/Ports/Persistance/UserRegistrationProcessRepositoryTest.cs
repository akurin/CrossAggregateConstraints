using System;
using System.Threading.Tasks;
using CrossAggregateConstraints.Domain;
using CrossAggregateConstraints.Ports.Persistance;
using CrossAggregateConstraints.Ports.Persistance.Repositories;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Embedded;
using EventStore.Core;
using NSpec;
using Optional;
using Optional.Unsafe;

namespace CrossAggregateConstraints.Tests.Ports.Persistance
{
    public sealed class UserRegistrationProcessRepositoryTest : nspec
    {
        private void before_each()
        {
            _node = TestEventStore.StartEmbedded();
            _connection = EmbeddedEventStoreConnection.Create(_node);
            _connection.ConnectAsync().Wait();

            var crossAggregateConstraints = new FakeCrossAggregateConstraints();
            var userRegistrationProcessFactory = new UserRegistrationProcessFactory(crossAggregateConstraints);
            _sut = new UserRegistrationProcessRepository(_connection, new EventSerializer(), userRegistrationProcessFactory);
        }

        private void after_each()
        {
            _connection?.Close();
            _node?.Stop();
        }

        private void describe_GetAsync()
        {
            context["when user registration process is present in repository"] = () =>
            {
                var presentProcess = new UserRegistrationProcess(
                    new UserRegistrationForm("someone@example.com"),
                    new FakeCrossAggregateConstraints());

                var result = new Option<UserRegistrationProcess>();

                before = () =>
                {
                    _sut.SaveAsync(presentProcess).Wait();
                    result = _sut.GetAsync(presentProcess.UserId).Result;
                };

                it["returns not none"] = () => result.HasValue.should_be_true();
                it["returns correct user registration process"] = () =>
                {
                    result.ValueOrFailure().should_be(presentProcess.UserId);
                    result.ValueOrFailure().RegistrationForm.Email.should_be(presentProcess.RegistrationForm.Email);
                };
            };

            context["when user registration process is not present in repository"] = () =>
            {
                var result = new Option<UserRegistrationProcess>();

                before = () => result = _sut.GetAsync(Guid.NewGuid()).Result;
                it["returns none"] = () => result.HasValue.should_be_false();
            };
        }

        private ClusterVNode _node;
        private IEventStoreConnection _connection;
        private IUserRegistrationProcessRepository _sut;

        private class FakeCrossAggregateConstraints : ICrossAggregateConstraints
        {
            public Task<bool> AddAsync(Guid userId, UserRegistrationForm registrationForm)
            {
                throw new NotImplementedException();
            }
        }
    }
}