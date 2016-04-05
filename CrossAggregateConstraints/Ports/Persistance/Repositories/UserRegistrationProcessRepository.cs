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
    public class UserRegistrationProcessRepository : IUserRegistrationProcessRepository
    {
        private readonly IEventStoreConnection _connection;
        private readonly IEventSerializer _eventSerializer;

        public UserRegistrationProcessRepository(
            IEventStoreConnection connection,
            IEventSerializer eventSerializer)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));
            if (eventSerializer == null) throw new ArgumentNullException(nameof(eventSerializer));

            _connection = connection;
            _eventSerializer = eventSerializer;
        }

        public async Task<SaveResult> SaveAsync(UserRegistrationProcess process)
        {
            if (process == null) throw new ArgumentNullException(nameof(process));

            var eventsData = process.GetPendingEvents().Select(_eventSerializer.ToEventData);

            try
            {
                await _connection.AppendToStreamAsync(
                    StreamBy(process.UserId),
                    process.Version == 0
                        ? ExpectedVersion.NoStream
                        : process.Version - 1,
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
            return "registrationProcess-" + userId;
        }

        public async Task<Option<UserRegistrationProcess>> GetAsync(Guid userId)
        {
            var resolvedEvents = await _connection.ReadSreamEventsAsync(StreamBy(userId));
            var eventsBytes = resolvedEvents.Select(e => e.OriginalEvent);
            var events = eventsBytes
                .Select(_eventSerializer.FromEventData)
                .Select(maybeEvent => maybeEvent.ValueOrFailure())
                .ToList();

            return events.Any()
                ? new UserRegistrationProcess(events).Some()
                : Option.None<UserRegistrationProcess>();
        }
    }
}