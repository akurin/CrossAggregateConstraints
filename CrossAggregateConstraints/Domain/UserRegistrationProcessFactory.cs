using System;
using System.Collections.Generic;
using CrossAggregateConstraints.Infrastructure.EventSourcing;

namespace CrossAggregateConstraints.Domain
{
    public sealed class UserRegistrationProcessFactory : IUserRegistrationProcessFactory
    {
        private readonly ICrossAggregateConstraints _crossAggregateConstraints;

        public UserRegistrationProcessFactory(ICrossAggregateConstraints crossAggregateConstraints)
        {
            if (crossAggregateConstraints == null) throw new ArgumentNullException(nameof(crossAggregateConstraints));

            _crossAggregateConstraints = crossAggregateConstraints;
        }

        public UserRegistrationProcess Create(UserRegistrationForm registrationForm)
        {
            return new UserRegistrationProcess(registrationForm, _crossAggregateConstraints);
        }

        public UserRegistrationProcess Create(IEnumerable<IEvent> events)
        {
            return new UserRegistrationProcess(events, _crossAggregateConstraints);
        }
    }
}