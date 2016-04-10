using System;
using CrossAggregateValidation.Adapters.Persistance;
using CrossAggregateValidation.Adapters.Persistance.Constraints;
using CrossAggregateValidation.Domain;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Embedded;
using EventStore.Core;
using NSpec;

namespace CrossAggregateValidation.Tests.Adapters.Persistance.Constraints
{
    public class UseryByEmailInMemoryIndexSpec : nspec
    {
        private IEventStoreConnection _connection;
        private ClusterVNode _node;
        private IndexResult _result;
        private UserByEmailIndex _sut;

        private void before_each()
        {
            _node = EmbeddedEventStore.Start();
            _connection = EmbeddedEventStoreConnection.Create(_node);
            _connection.ConnectAsync().Wait();

            _sut = new UserByEmailIndex(_connection, new EventSerializer());
        }

        private void after_each()
        {
            _connection?.Close();
            _node?.Stop();
        }

        private void describe_AddAsync()
        {
            var userId = Guid.NewGuid();
            const string email = "johndow@example.com";

            context["when index does not contain email"] = () =>
            {
                before = () => _result = _sut.AddAsync(email, userId).Result;

                it["returns EmailAccepted"] = () => _result.should_be(IndexResult.EmailAccepted);
                it["is idempotent"] = () =>
                {
                    _result = _sut.AddAsync(email, userId).Result;
                    _result.should_be(IndexResult.EmailAccepted);
                };
            };

            context["email has been associated with another user"] = () =>
            {
                before = () =>
                {
                    _sut.AddAsync(email, userId).Wait();
                    _result = _sut.AddAsync(email, Guid.NewGuid()).Result;
                };

                it["returns EmailRejected"] = () => _result.should_be(IndexResult.EmailRejected);
            };
        }
    }
}