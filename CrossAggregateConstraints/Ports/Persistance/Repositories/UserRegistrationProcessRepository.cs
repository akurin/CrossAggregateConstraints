using System;
using System.Linq;
using System.Threading.Tasks;
using CrossAggregateConstraints.Domain;
using EventStore.ClientAPI;
using Optional;
using Optional.Unsafe;

namespace CrossAggregateConstraints.Ports.Persistance.Repositories
{
    public sealed class UserRegistrationProcessRepository : IUserRegistrationProcessRepository
    {
        private readonly IEventStoreConnection _connection;
        private readonly IEventSerializer _eventSerializer;
        private readonly IUserRegistrationProcessFactory _userRegistrationProcessFactory;

        public UserRegistrationProcessRepository(
            IEventStoreConnection connection,
            IEventSerializer eventSerializer,
            IUserRegistrationProcessFactory userRegistrationProcessFactory)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));
            if (eventSerializer == null) throw new ArgumentNullException(nameof(eventSerializer));
            if (userRegistrationProcessFactory == null)
                throw new ArgumentNullException(nameof(userRegistrationProcessFactory));

            _connection = connection;
            _eventSerializer = eventSerializer;
            _userRegistrationProcessFactory = userRegistrationProcessFactory;
        }

        public Task SaveAsync(UserRegistrationProcess registrationProcess)
        {
            var eventsData = registrationProcess.GetEvents().Select(_eventSerializer.ToEventData);
            return _connection.AppendToStreamAsync(
                StreamBy(registrationProcess.UserId),
                ExpectedVersion.Any,
                eventsData);
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
                ? _userRegistrationProcessFactory.Create(events).Some()
                : Option.None<UserRegistrationProcess>();
        }
    }
}