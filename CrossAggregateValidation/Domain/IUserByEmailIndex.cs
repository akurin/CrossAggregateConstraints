using System;
using System.Threading.Tasks;

namespace CrossAggregateValidation.Domain
{
    public interface IUserByEmailIndex
    {
        Task<IndexResult> AddAsync(string email, Guid userId);
    }
}