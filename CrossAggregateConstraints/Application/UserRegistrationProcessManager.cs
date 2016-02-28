using System;
using System.Threading.Tasks;
using CrossAggregateConstraints.Domain;
using Optional.Unsafe;

namespace CrossAggregateConstraints.Application
{
    public sealed class UserRegistrationProcessManager
    {
        private readonly IUserRegistrationProcessFactory _userRegistrationProcessFactory;
        private readonly IUserRegistrationProcessRepository _userRegistrationProcessRepository;
        private readonly IUserRepository _userRepository;

        public UserRegistrationProcessManager(
            IUserRegistrationProcessFactory userRegistrationProcessFactory,
            IUserRegistrationProcessRepository userRegistrationProcessRepository,
            IUserRepository userRepository)
        {
            if (userRegistrationProcessFactory == null)
                throw new ArgumentNullException(nameof(userRegistrationProcessFactory));
            if (userRegistrationProcessRepository == null) throw new ArgumentNullException(nameof(userRegistrationProcessRepository));
            if (userRepository == null) throw new ArgumentNullException(nameof(userRepository));

            _userRegistrationProcessFactory = userRegistrationProcessFactory;
            _userRegistrationProcessRepository = userRegistrationProcessRepository;
            _userRepository = userRepository;
        }

        public async Task<Guid> StartRegistrationAsync(UserRegistrationForm registrationForm)
        {
            if (registrationForm == null) throw new ArgumentNullException(nameof(registrationForm));

            var userRegistrationProcess = _userRegistrationProcessFactory.Create(registrationForm);
            await _userRegistrationProcessRepository.SaveAsync(userRegistrationProcess);
            return userRegistrationProcess.UserId;
        }

        public async Task HandleAsync(UserRegistrationProcessStarted @event)
        {
            if (@event == null) throw new ArgumentNullException(nameof(@event));

            var process = await GetProcessOrExceptionAsync(@event.UserId);
            await process.AddCrossUserConstraints();
            await _userRegistrationProcessRepository.SaveAsync(process);
        }

        private async Task<UserRegistrationProcess> GetProcessOrExceptionAsync(Guid userId)
        {
            var maybeProcess = await _userRegistrationProcessRepository.GetAsync(userId);
            var errorMessage = $"Could not find user registration process for user {{userId}}. Repository seems to be corrupted.";
            return maybeProcess.ValueOrFailure(errorMessage);
        }

        public async Task HandleAsync(CrossUserConstraintsAdded @event)
        {
            if (@event == null) throw new ArgumentNullException(nameof(@event));

            var process = await GetProcessOrExceptionAsync(@event.UserId);
            var registrationForm = process.RegistrationForm;
            var user = new User(@event.UserId, registrationForm.Email);
            await _userRepository.SaveAsync(user);
        }

        public async Task HandleAsync(CrossUserConstraintsFailed @event)
        {
            if (@event == null) throw new ArgumentNullException(nameof(@event));

            var process = (await _userRegistrationProcessRepository.GetAsync(@event.UserId)).ValueOrFailure();
            process.Fail();
            await _userRegistrationProcessRepository.SaveAsync(process);
        }

        public async Task HandleAsync(UserCreated @event)
        {
            if (@event == null) throw new ArgumentNullException(nameof(@event));

            var process = await GetProcessOrExceptionAsync(@event.UserId);
            process.Succeed();
            await _userRegistrationProcessRepository.SaveAsync(process);
        }
    }
}