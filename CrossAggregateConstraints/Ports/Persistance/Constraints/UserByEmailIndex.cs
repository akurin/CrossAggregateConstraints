using System;
using System.Threading.Tasks;
using CrossAggregateConstraints.Domain;
using CrossAggregateConstraints.Domain.Events;
using CrossAggregateConstraints.Ports.Persistance.Repositories;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Exceptions;
using Optional;
using Optional.Unsafe;

namespace CrossAggregateConstraints.Ports.Persistance.Constraints
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
            while (true)
            {
                var maybeEvent = await ReadUserByEmailIndexedEventAsync(email);
                if (maybeEvent.HasValue)
                {
                    return maybeEvent.ValueOrFailure().UserId == userId
                        ? IndexResult.EmailAccepted
                        : IndexResult.EmailRejected;
                }

                try
                {
                    var userByEmailAdded = new UserIndexedByEmail(userId);
                    var eventData = _eventSerializer.ToEventData(userByEmailAdded);
                    await _connection.AppendToStreamAsync(
                        StreamBy(email),
                        ExpectedVersion.EmptyStream,
                        eventData);

                    return IndexResult.EmailAccepted;
                }
                catch (WrongExpectedVersionException)
                {
                }
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
                        var message = $"Expected event of type {typeof (UserIndexedByEmail).FullName} " +
                                      "but was '{@event.GetType().FullName}'";
                        throw new Exception(message);
                    }

                    return @event as UserIndexedByEmail;
                });
        }
    }
}