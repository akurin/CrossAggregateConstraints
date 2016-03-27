using System;
using System.Linq;
using System.Threading.Tasks;
using CrossAggregateConstraints.Domain;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Exceptions;
using Optional;
using Optional.Unsafe;

namespace CrossAggregateConstraints.Ports.Persistance.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IEventStoreConnection _connection;
        private readonly IEventSerializer _eventSerializer;

        public UserRepository(IEventStoreConnection connection, IEventSerializer eventSerializer)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));
            if (eventSerializer == null) throw new ArgumentNullException(nameof(eventSerializer));

            _connection = connection;
            _eventSerializer = eventSerializer;
        }

        public async Task<SaveResult> SaveAsync(User user)
        {
            var eventsData = user.GetEvents().Select(_eventSerializer.ToEventData);
            try
            {
                await _connection.AppendToStreamAsync(
                    StreamBy(user.Id),
                    user.Version == 0
                        ? ExpectedVersion.NoStream
                        : user.Version - 1,
                    eventsData);

                return SaveResult.Success;
            }
            catch (WrongExpectedVersionException)
            {
                return SaveResult.WrongExpextedVersion;
            }
        }

        private static string StreamBy(Guid userId)
        {
            return "user_" + userId;
        }

        public async Task<Option<User>> GetAsync(Guid id)
        {
            var resolvedEvents = await _connection.ReadSreamEventsAsync(StreamBy(id));
            var eventsBytes = resolvedEvents.Select(e => e.OriginalEvent);
            var events = eventsBytes
                .Select(_eventSerializer.FromEventData)
                .Select(maybeEvent => maybeEvent.ValueOrFailure())
                .ToList();

            return events.Any()
                ? new User(events).Some()
                : Option.None<User>();
        }
    }
}