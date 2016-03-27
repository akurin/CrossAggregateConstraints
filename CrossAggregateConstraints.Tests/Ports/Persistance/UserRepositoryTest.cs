using System;
using CrossAggregateConstraints.Domain;
using CrossAggregateConstraints.Ports.Persistance;
using CrossAggregateConstraints.Ports.Persistance.Repositories;
using CrossAggregateConstraints.Tests.Domain;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Embedded;
using EventStore.Core;
using NSpec;
using Optional;
using Optional.Unsafe;

namespace CrossAggregateConstraints.Tests.Ports.Persistance
{
    public class UserRepositoryTest : nspec
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

            _sut = new UserRepository(_connection, new EventSerializer());
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