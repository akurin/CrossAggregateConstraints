using System;
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
    public sealed class UserRepositoryTest : nspec
    {
        private void before_each()
        {
            _node = TestEventStore.StartEmbedded();
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
            context["when user is present in repository"] = () =>
            {
                var presentUser = new User(Guid.NewGuid(), "someone@example.com");
                var result = new Option<User>();

                before = () =>
                {
                    _sut.SaveAsync(presentUser).Wait();
                    result = _sut.GetAsync(presentUser.Id).Result;
                };

                it["returns not none"] = () => result.HasValue.should_be_true();
                it["returns correct user"] = () =>
                {
                    result.ValueOrFailure().Id.should_be(presentUser.Id);
                    result.ValueOrFailure().Email.should_be(presentUser.Email);
                };
            };

            context["when user is not present in repository"] = () =>
            {
                var result = new Option<User>();

                before = () => result = _sut.GetAsync(Guid.NewGuid()).Result;
                it["returns none"] = () => result.HasValue.should_be_false();
            };
        }

        private ClusterVNode _node;
        private IEventStoreConnection _connection;
        private IUserRepository _sut;
    }
}