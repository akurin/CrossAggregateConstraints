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
    public class UserRegistrationProcessRepositorySpec : nspec
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

            var eventSerializer = JsonNetEventSerializer.CreateForAssembly(typeof(IEvent).Assembly);
            _sut = new UserRegistrationProcessRepository(_connection, eventSerializer);
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
                var presentProcess = UserRegistrationProcessMother.InRegistrationStartedState(Guid.NewGuid());

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