using System;
using System.Threading.Tasks;
using CrossAggregateValidation.Adapters.Persistance.Repositories;
using CrossAggregateValidation.Domain;
using CrossAggregateValidation.Domain.Events;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Exceptions;
using Optional;
using Optional.Unsafe;

namespace CrossAggregateValidation.Adapters.Persistance.Constraints
{
    public class UserByEmailIndex : IUserByEmailIndex
    {
        private readonly IEventStoreConnection _connection;
        private readonly IEventSerializer _eventSerializer;

        public UserByEmailIndex(IEventStoreConnection connection, IEventSerializer eventSerializer)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));
            if (eventSerializer == null) throw new ArgumentNullException(nameof(eventSerializer));

            _connection = connection;
            _eventSerializer = eventSerializer;
        }

        public async Task<IndexResult> AddAsync(string email, Guid userId)
        {
            var userByEmailAdded = new UserIndexedByEmail(userId);
            var eventData = _eventSerializer.ToEventData(userByEmailAdded);

            try
            {
                await _connection.AppendToStreamAsync(
                    StreamBy(email),
                    ExpectedVersion.EmptyStream,
                    eventData);

                return IndexResult.EmailAccepted;
            }
            catch (WrongExpectedVersionException)
            {
                var maybeEvent = await ReadUserByEmailIndexedEventAsync(email);
                return maybeEvent.Match(
                    e => e.UserId == userId
                        ? IndexResult.EmailAccepted
                        : IndexResult.EmailRejected,
                    () =>
                    {
                        throw new Exception(
                            "Unexpected EventStore behavior: append operation was rejected with " +
                            "WrongExpectedVersionException, but the stream seems to be empty");
                    });
            }
        }

        private static string StreamBy(string email)
        {
            return "userByEmailIndex-" + email;
        }

        private async Task<Option<UserIndexedByEmail>> ReadUserByEmailIndexedEventAsync(string email)
        {
            var maybeResolvedEvent = await _connection.ReadLastStreamEvent(StreamBy(email));
            return maybeResolvedEvent.Map(
                resolvedEvent =>
                {
                    var fromEventData = _eventSerializer.FromEventData(resolvedEvent.OriginalEvent);
                    if (!fromEventData.HasValue)
                        throw new Exception($"Could not read event of type {resolvedEvent.OriginalEvent.EventType}");

                    var @event = fromEventData.ValueOrFailure();
                    if (!(@event is UserIndexedByEmail))
                    {
                        var message = $"Expected event of type {typeof(UserIndexedByEmail).FullName} " +
                                      "but was '{@event.GetType().FullName}'";
                        throw new Exception(message);
                    }

                    return @event as UserIndexedByEmail;
                });
        }
    }
}