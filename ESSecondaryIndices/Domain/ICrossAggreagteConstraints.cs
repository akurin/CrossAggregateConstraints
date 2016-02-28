using System;
using System.Threading.Tasks;

namespace ESSecondaryIndices.Domain
{
    public interface ICrossAggregateConstraints
    {
        Task<bool> AddAsync(Guid userId, UserRegistrationForm registrationForm);
    }
}