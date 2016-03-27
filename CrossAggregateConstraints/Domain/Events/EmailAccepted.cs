using System;

namespace CrossAggregateConstraints.Domain.Events
{
    public class EmailAccepted : IEvent
    {
        public Guid UserId { get; }

        public EmailAccepted(Guid userId)
        {
            UserId = userId;
        }
    }
}