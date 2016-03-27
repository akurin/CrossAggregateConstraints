using System;

namespace CrossAggregateConstraints.Domain.Events
{
    public class EmailAccepted : IEvent
    {
        public EmailAccepted(Guid userId)
        {
            UserId = userId;
        }

        public Guid UserId { get; }
    }
}