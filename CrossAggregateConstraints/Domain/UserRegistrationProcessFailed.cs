using System;
using CrossAggregateConstraints.Infrastructure.EventSourcing;

namespace CrossAggregateConstraints.Domain
{
    public class UserRegistrationProcessFailed : IEvent
    {
        public Guid UserId { get; }

        public UserRegistrationProcessFailed(Guid userId)
        {
            UserId = userId;
        }
    }
}