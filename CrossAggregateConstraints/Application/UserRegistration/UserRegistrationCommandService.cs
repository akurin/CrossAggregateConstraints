using System;
using System.Threading.Tasks;
using CrossAggregateConstraints.Domain;

namespace CrossAggregateConstraints.Application.UserRegistration
{
    public class UserRegistrationCommandService
    {
        private readonly IUserRegistrationProcessRepository _userRegistrationProcessRepository;

        public UserRegistrationCommandService(IUserRegistrationProcessRepository userRegistrationProcessRepository)
        {
            if (userRegistrationProcessRepository == null)
                throw new ArgumentNullException(nameof(userRegistrationProcessRepository));

            _userRegistrationProcessRepository = userRegistrationProcessRepository;
        }

        public async Task<Guid> StartRegistrationAsync(UserRegistrationForm registrationForm)
        {
            if (registrationForm == null) throw new ArgumentNullException(nameof(registrationForm));

            var userRegistrationProcess = new UserRegistrationProcess(registrationForm);
            await _userRegistrationProcessRepository.SaveAsync(userRegistrationProcess);
            return userRegistrationProcess.UserId;
        }
    }
}