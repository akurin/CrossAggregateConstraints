using System;

namespace CrossAggregateConstraints.Domain.Events
{
    public class UserRegistrationSucceeded : IEvent
    {
        public Guid UserId { get; }

        public UserRegistrationSucceeded(Guid userId)
        {
            UserId = userId;
        }
    }
}