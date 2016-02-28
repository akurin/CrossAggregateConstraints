using System.Collections.Generic;
using ESSecondaryIndices.Infrastructure.EventSourcing;

namespace ESSecondaryIndices.Domain
{
    public interface IUserRegistrationProcessFactory
    {
        UserRegistrationProcess Create(UserRegistrationForm registrationForm);
        UserRegistrationProcess Create(IEnumerable<IEvent> events);
    }
}