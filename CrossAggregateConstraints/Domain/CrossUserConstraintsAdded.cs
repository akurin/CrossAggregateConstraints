using System;
using CrossAggregateConstraints.Infrastructure.EventSourcing;

namespace CrossAggregateConstraints.Domain
{
    public sealed class CrossUserConstraintsAdded : IEvent
    {
        public Guid UserId { get; }

        public CrossUserConstraintsAdded(Guid userId)
        {
            UserId = userId;
        }
    }
}