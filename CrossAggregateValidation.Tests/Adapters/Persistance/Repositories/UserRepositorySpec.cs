using System;
using CrossAggregateValidation.Adapters.Persistance;
using CrossAggregateValidation.Adapters.Persistance.JsonNetEventSerialization;
using CrossAggregateValidation.Adapters.Persistance.Repositories;
using CrossAggregateValidation.Domain;
using CrossAggregateValidation.Tests.Domain;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Embedded;
using EventStore.Core;
using NSpec;
using Optional;
using Optional.Unsafe;

namespace CrossAggregateValidation.Tests.Adapters.Persistance.Repositories
{
    public class UserRepositorySpec : nspec
    {
        private IEventStoreConnection _connection;
        private ClusterVNode _node;
        private IUserRepository _sut;
        private Option<User> _result;

        private void before_each()
        {
            _node = EmbeddedEventStore.Start();
            _connection = EmbeddedEventStoreConnection.Create(_node);
            _connection.ConnectAsync().Wait();

            var eventSerializer = JsonNetEventSerializer.CreateForAssembly(typeof(IEvent).Assembly);
            _sut = new UserRepository(_connection, eventSerializer);
        }

        private void after_each()
        {
            _connection?.Close();
            _node?.Stop();
        }

        private void describe_GetAsync()
        {
            var userRegistrationForm = UserRegistrationFormMother.JohnDow();

            context["when user is present in repository"] = () =>
            {
                var presentUser = new User(Guid.NewGuid(), userRegistrationForm);

                before = () =>
                {
                    _sut.SaveAsync(presentUser).Wait();
                    _result = _sut.GetAsync(presentUser.Id).Result;
                };

                it["returns not none"] = () => _result.HasValue.should_be_true();
                it["returns user with correct Id"] = () =>
                    _result.ValueOrFailure().Id.should_be(presentUser.Id);
            };

            context["when user is not present in repository"] = () =>
            {
                var result = new Option<User>();

                before = () => result = _sut.GetAsync(Guid.NewGuid()).Result;
                it["returns none"] = () => result.HasValue.should_be_false();
            };
        }
    }
}