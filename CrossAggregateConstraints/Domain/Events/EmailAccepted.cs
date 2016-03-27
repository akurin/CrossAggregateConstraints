using System;
using CrossAggregateConstraints.Infrastructure.EventSourcing;

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