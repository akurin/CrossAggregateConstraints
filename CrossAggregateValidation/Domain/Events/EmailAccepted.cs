using System;

namespace CrossAggregateValidation.Domain.Events
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