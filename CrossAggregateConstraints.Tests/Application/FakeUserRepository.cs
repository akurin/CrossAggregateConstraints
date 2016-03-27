using System;
using System.Threading.Tasks;
using CrossAggregateConstraints.Domain;
using Optional;

namespace CrossAggregateConstraints.Tests.Application
{
    internal class FakeUserRepository : IUserRepository
    {
        private readonly FakeEventStore _eventStoreStub;

        public FakeUserRepository(FakeEventStore eventStoreStub)
        {
            if (eventStoreStub == null) throw new ArgumentNullException(nameof(eventStoreStub));

            _eventStoreStub = eventStoreStub;
        }

        public Task<SaveResult> SaveAsync(User user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            var streamId = StreamId(user.Id);
            var result = _eventStoreStub.Save(streamId, user.GetEvents(), user.Version);
            return Task.FromResult(result);
        }

        private static string StreamId(Guid userId)
        {
            return "user_" + userId;
        }

        public Task<Option<User>> GetAsync(Guid userId)
        {
            var streamId = StreamId(userId);
            var events = _eventStoreStub.GetByStreamId(streamId);
            var result = new User(events);
            return Task.FromResult(result.Some());
        }
    }
}