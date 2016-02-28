using System.Collections.Generic;
using CrossAggregateConstraints.Infrastructure.EventSourcing;

namespace CrossAggregateConstraints.Domain
{
    public interface IUserRegistrationProcessFactory
    {
        UserRegistrationProcess Create(UserRegistrationForm registrationForm);
        UserRegistrationProcess Create(IEnumerable<IEvent> events);
    }
}