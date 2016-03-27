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
    public class UserRegistrationProcessRepositoryTest : nspec
    {
        private ClusterVNode _node;
        private IEventStoreConnection _connection;
        private UserRegistrationProcessRepository _sut;
        private Option<UserRegistrationProcess> _result;

        private void before_each()
        {
            _node = EmbeddedEventStore.Start();
            _connection = EmbeddedEventStoreConnection.Create(_node);
            _connection.ConnectAsync().Wait();

            _sut = new UserRegistrationProcessRepository(_connection, new EventSerializer());
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
                var presentProcess = UserRegistrationProcessMother.InCreatedState(Guid.NewGuid());

                before = () =>
                {
                    _sut.SaveAsync(presentProcess).Wait();
                    _result = _sut.GetAsync(presentProcess.UserId).Result;
                };

                it["returns not none"] = () => _result.HasValue.should_be_true();
                it["returns user registration process with correct UserId"] =
                    () => { _result.ValueOrFailure().UserId.should_be(presentProcess.UserId); };
            };

            context["when user registration process is not present in repository"] = () =>
            {
                before = () => _result = _sut.GetAsync(Guid.NewGuid()).Result;
                it["returns none"] = () => _result.HasValue.should_be_false();
            };
        }
    }
}