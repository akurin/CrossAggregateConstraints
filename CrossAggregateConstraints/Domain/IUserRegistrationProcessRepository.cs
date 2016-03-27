using System;
using System.Threading.Tasks;
using Optional;

namespace CrossAggregateConstraints.Domain
{
    public interface IUserRegistrationProcessRepository
    {
        Task<SaveResult> SaveAsync(UserRegistrationProcess process);
        Task<Option<UserRegistrationProcess>> GetAsync(Guid userId);
    }
}