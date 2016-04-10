using System;

namespace CrossAggregateValidation.Domain.Events
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