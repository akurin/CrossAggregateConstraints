using System;
using CrossAggregateConstraints.Infrastructure.EventSourcing;

namespace CrossAggregateConstraints.Domain
{
    public class UserRegistrationProcessSucceeded : IEvent
    {
        public Guid UserId { get; }

        public UserRegistrationProcessSucceeded(Guid userId)
        {
            UserId = userId;
        }
    }
}