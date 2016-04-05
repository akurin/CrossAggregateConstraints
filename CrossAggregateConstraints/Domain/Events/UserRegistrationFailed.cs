using System;

namespace CrossAggregateConstraints.Domain.Events
{
    public class UserRegistrationFailed : IEvent
    {
        public UserRegistrationFailed(Guid userId)
        {
            UserId = userId;
        }

        public Guid UserId { get; }
    }
}