using System.Linq;
using CrossAggregateConstraints.Application.UserRegistration;
using CrossAggregateConstraints.Domain.Events;
using CrossAggregateConstraints.Tests.Domain;
using NSpec;

namespace CrossAggregateConstraints.Tests.Application
{
    public class UserRegistrationCommandServiceSpec : nspec
    {
        private UserRegistrationCommandService _sut;
        private FakeEventStore _eventStore;

        private void before_each()
        {
            var fakeEventStore = new FakeEventStore();
            var fakeUserRegistrationProcessRepository = new FakeUserRegistrationProcessRepository(fakeEventStore);

            _sut = new UserRegistrationCommandService( fakeUserRegistrationProcessRepository);

            _eventStore = fakeEventStore;
        }

        private void describe_StartRegistrationAsync()
        {
            before = () => _sut.StartRegistrationAsync(UserRegistrationFormMother.JohnDow()).Wait();

            it["saves UserRegistrationStarted event"] = () =>
                _eventStore.LastSavedEvents.Single().should_cast_to<UserRegistrationStarted>();
        }
    }
}