using System;
using System.Threading.Tasks;

namespace CrossAggregateConstraints.Domain
{
    public interface IUserByEmailIndex
    {
        Task<IndexResult> AddAsync(string email, Guid userId);
    }
}