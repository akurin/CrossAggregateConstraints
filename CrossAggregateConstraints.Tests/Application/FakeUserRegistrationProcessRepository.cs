using System;
using System.Threading.Tasks;
using CrossAggregateConstraints.Domain;
using Optional;

namespace CrossAggregateConstraints.Tests.Application
{
    internal class FakeUserRegistrationProcessRepository : IUserRegistrationProcessRepository
    {
        private readonly FakeEventStore _eventStoreStub;

        public FakeUserRegistrationProcessRepository(FakeEventStore eventStoreStub)
        {
            if (eventStoreStub == null) throw new ArgumentNullException(nameof(eventStoreStub));

            _eventStoreStub = eventStoreStub;
        }

        public Task<SaveResult> SaveAsync(UserRegistrationProcess process)
        {
            var streamId = StreamBy(process.UserId);
            var result = _eventStoreStub.Save(
                streamId,
                process.GetPendingEvents(),
                process.Version);

            return Task.FromResult(result);
        }

        private static string StreamBy(Guid userId)
        {
            return "userRegistrationProcess-" + userId;
        }

        public Task<Option<UserRegistrationProcess>> GetAsync(Guid userId)
        {
            var streamId = StreamBy(userId);
            var events = _eventStoreStub.GetByStreamId(streamId);
            var result = new UserRegistrationProcess(events);
            return Task.FromResult(result.Some());
        }
    }
}