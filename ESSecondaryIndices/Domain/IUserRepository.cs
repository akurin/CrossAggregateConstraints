using System;
using System.Threading.Tasks;
using Optional;

namespace ESSecondaryIndices.Domain
{
    public interface IUserRepository
    {
        Task<Option<User>> GetAsync(Guid userId);
        Task SaveAsync(User user);
    }
}