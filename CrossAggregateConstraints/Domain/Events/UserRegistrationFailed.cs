using System;
using CrossAggregateConstraints.Infrastructure.EventSourcing;

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