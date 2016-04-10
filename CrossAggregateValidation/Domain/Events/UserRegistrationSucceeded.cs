using System;

namespace CrossAggregateValidation.Domain.Events
{
    public class UserRegistrationSucceeded : IEvent
    {
        public UserRegistrationSucceeded(Guid userId)
        {
            UserId = userId;
        }

        public Guid UserId { get; }
    }
}