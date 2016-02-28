using System;
using System.Threading.Tasks;

namespace CrossAggregateConstraints.Domain
{
    public interface ICrossAggregateConstraints
    {
        Task<bool> AddAsync(Guid userId, UserRegistrationForm registrationForm);
    }
}