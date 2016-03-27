using System;
using CrossAggregateConstraints.Infrastructure.EventSourcing;

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