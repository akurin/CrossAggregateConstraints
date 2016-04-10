using System;
using System.Threading.Tasks;
using CrossAggregateValidation.Domain;
using CrossAggregateValidation.Domain.Events;
using Optional.Unsafe;

namespace CrossAggregateValidation.Application.EventHandling
{
    public class UserRegistrationEventHandler
    {
        private readonly IUserRegistrationProcessRepository _userRegistrationProcessRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUserByEmailIndex _userByEmailIndex;

        public UserRegistrationEventHandler(
            IUserRegistrationProcessRepository userRegistrationProcessRepository,
            IUserRepository userRepository,
            IUserByEmailIndex userByEmailIndex)
        {
            if (userRegistrationProcessRepository == null)
                throw new ArgumentNullException(nameof(userRegistrationProcessRepository));

            if (userRepository == null) throw new ArgumentNullException(nameof(userRepository));
            if (userByEmailIndex == null) throw new ArgumentNullException(nameof(userByEmailIndex));

            _userRegistrationProcessRepository = userRegistrationProcessRepository;
            _userRepository = userRepository;
            _userByEmailIndex = userByEmailIndex;
        }

        public async Task HandleAsync(UserRegistrationStarted @event)
        {
            var indexResult = await _userByEmailIndex.AddAsync(@event.Form.Email, @event.UserId);
            var process = await GetProcessOrExceptionAsync(@event.UserId);

            switch (indexResult)
            {
                case IndexResult.EmailAccepted:
                    process.HandleEmailAccepted();
                    break;
                case IndexResult.EmailRejected:
                    process.HandleEmailRejected();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            await _userRegistrationProcessRepository.SaveAsync(process);
        }

        private async Task<UserRegistrationProcess> GetProcessOrExceptionAsync(Guid userId)
        {
            var maybeProcess = await _userRegistrationProcessRepository.GetAsync(userId);
            var errorMessage =
                $"Could not find user registration process for user {userId}. Repository seems to be corrupted.";
            return maybeProcess.ValueOrFailure(errorMessage);
        }

        public async Task HandleAsync(EmailAccepted @event)
        {
            if (@event == null) throw new ArgumentNullException(nameof(@event));

            var process = await GetProcessOrExceptionAsync(@event.UserId);
            var user = process.CreateUser();
            await _userRepository.SaveAsync(user);
        }

        public async Task HandleAsync(UserCreated @event)
        {
            if (@event == null) throw new ArgumentNullException(nameof(@event));

            var process = await GetProcessOrExceptionAsync(@event.UserId);
            process.HandleUserCreated();
            await _userRegistrationProcessRepository.SaveAsync(process);
        }
    }
}