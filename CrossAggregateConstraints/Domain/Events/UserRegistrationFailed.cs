using System;

namespace CrossAggregateConstraints.Domain.Events
{
    public class UserRegistrationFailed : IEvent
    {
        public Guid UserId { get; }

        public UserRegistrationFailed(Guid userId)
        {
            UserId = userId;
        }
    }
}