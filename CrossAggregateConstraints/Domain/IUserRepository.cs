using System;
using System.Threading.Tasks;
using Optional;

namespace CrossAggregateConstraints.Domain
{
    public interface IUserRepository
    {
        Task<SaveResult> SaveAsync(User user);
        Task<Option<User>> GetAsync(Guid userId);
    }
}