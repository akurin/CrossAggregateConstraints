using System;
using System.Linq;
using CrossAggregateConstraints.Application.EventHandling;
using CrossAggregateConstraints.Domain;
using CrossAggregateConstraints.Domain.Events;
using CrossAggregateConstraints.Tests.Domain;
using NSpec;

namespace CrossAggregateConstraints.Tests.Application
{
    public class UserRegistrationEventHandlerSpec : nspec
    {
        private UserRegistrationEventHandler _sut;
        private FakeUserRegistrationProcessRepository _userRegistrationProcessRepository;
        private FakeEventStore _eventStore;
        private UserByEmailIndexStub _userByEmailIndex;
        private readonly Guid _userId = Guid.NewGuid();

        private void before_each()
        {
            var userByEmailIndexStub = new UserByEmailIndexStub();

            var fakeEventStore = new FakeEventStore();
            var fakeUserRegistrationProcessRepository = new FakeUserRegistrationProcessRepository(fakeEventStore);
            var fakeUserRepository = new FakeUserRepository(fakeEventStore);

            _sut = new UserRegistrationEventHandler(
                fakeUserRegistrationProcessRepository,
                fakeUserRepository,
                userByEmailIndexStub);

            _userRegistrationProcessRepository = fakeUserRegistrationProcessRepository;
            _eventStore = fakeEventStore;
            _userByEmailIndex = userByEmailIndexStub;
        }

        private void describe_HandleAsync_UserRegistrationProcessCreated()
        {
            var userRegistrationProcessCreated = new UserRegistrationStarted(
                _userId,
                UserRegistrationFormMother.JohnDow());

            context["when user registration process is in Created state"] = () =>
            {
                before = () =>
                {
                    var process = UserRegistrationProcessMother.InCreatedState(_userId);
                    _userRegistrationProcessRepository.SaveAsync(process).Wait();
                };

                context["and user-by-email index accepts email"] = () =>
                {
                    before = () =>
                    {
                        _userByEmailIndex.SetResult(IndexResult.EmailAccepted);
                        _sut.HandleAsync(userRegistrationProcessCreated).Wait();
                    };

                    it["saves EmailAccepted event"] =
                        () => _eventStore.LastSavedEvents.Single().should_cast_to<EmailAccepted>();

                    it["is idempotent"] = () =>
                    {
                        _sut.HandleAsync(userRegistrationProcessCreated).Wait();
                        _eventStore.LastSavedEvents.should_be_empty();
                    };
                };

                context["and user-by-email index rejects email"] = () =>
                {
                    before = () =>
                    {
                        _userByEmailIndex.SetResult(IndexResult.EmailRejected);
                        _sut.HandleAsync(userRegistrationProcessCreated)
                            .Wait();
                    };

                    it["saves UserRegistrationFailed event"] = () =>
                        _eventStore.LastSavedEvents.Single().should_cast_to<UserRegistrationFailed>();

                    it["is idempotent"] = () =>
                    {
                        _sut.HandleAsync(userRegistrationProcessCreated).Wait();
                        _eventStore.LastSavedEvents.should_be_empty();
                    };
                };
            };
        }

        private void describe_HandleAsync_EmailAccepted()
        {
            var EmailAccepted = new EmailAccepted(_userId);

            context["when user registration process is in CreatingUser state"] = () =>
            {
                before = () =>
                {
                    var process = UserRegistrationProcessMother.InCreatingUserState(_userId);
                    _userRegistrationProcessRepository.SaveAsync(process).Wait();
                    _sut.HandleAsync(EmailAccepted).Wait();
                };

                it["saves UserCreated event"] =
                    () => _eventStore.LastSavedEvents.Single().should_cast_to<UserCreated>();

                it["is idempotent"] = () =>
                {
                    _sut.HandleAsync(EmailAccepted).Wait();
                    _eventStore.LastSavedEvents.should_be_empty();
                };
            };
        }

        private void describe_HandleAsync_UserCreated()
        {
            var userCreated = new UserCreated(_userId, UserRegistrationFormMother.JohnDow());

            context["when user registration process is in CreatingUser state"] = () =>
            {
                before = () =>
                {
                    var process = UserRegistrationProcessMother.InCreatingUserState(_userId);
                    _userRegistrationProcessRepository.SaveAsync(process).Wait();
                    _sut.HandleAsync(userCreated).Wait();
                };

                it["saves UserRegistrationSucceeded event"] =
                    () => _eventStore.LastSavedEvents.Single().should_cast_to<UserRegistrationSucceeded>();

                it["is idempotent"] = () =>
                {
                    _sut.HandleAsync(userCreated).Wait();
                    _eventStore.LastSavedEvents.should_be_empty();
                };
            };
        }
    }
}