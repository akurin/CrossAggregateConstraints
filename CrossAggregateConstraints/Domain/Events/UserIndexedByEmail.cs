using System;

namespace CrossAggregateConstraints.Domain.Events
{
    public class UserIndexedByEmail : IEvent
    {
        public Guid UserId { get; }

        public UserIndexedByEmail(Guid userId)
        {
            UserId = userId;
        }
    }
}