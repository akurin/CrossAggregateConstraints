using System;
using System.Threading.Tasks;
using CrossAggregateValidation.Domain;
using Optional;
using Optional.Linq;

namespace CrossAggregateValidation.Application.UserRegistration
{
    public class UserRegistrationQueryService
    {
        private readonly IUserRegistrationProcessRepository _userRegistrationProcessRepository;

        public UserRegistrationQueryService(IUserRegistrationProcessRepository userRegistrationProcessRepository)
        {
            if (userRegistrationProcessRepository == null)
                throw new ArgumentNullException(nameof(userRegistrationProcessRepository));

            _userRegistrationProcessRepository = userRegistrationProcessRepository;
        }

        public async Task<Option<UserRegstrationProcessQueryResult>> GetAsync(Guid userId)
        {
            var maybeProcess = await _userRegistrationProcessRepository.GetAsync(userId);
            return maybeProcess.Select(process => ToQueryResult(process.State));
        }

        private static UserRegstrationProcessQueryResult ToQueryResult(UserRegistrationProcessState state)
        {
            switch (state)
            {
                case UserRegistrationProcessState.Created:
                    return UserRegstrationProcessQueryResult.InProgress;
                case UserRegistrationProcessState.CreatingUser:
                    return UserRegstrationProcessQueryResult.InProgress;
                case UserRegistrationProcessState.Succeeded:
                    return UserRegstrationProcessQueryResult.Succeeded;
                case UserRegistrationProcessState.Failed:
                    return UserRegstrationProcessQueryResult.Failed;
                default:
                    throw new InvalidOperationException();
            }
        }
    }
}