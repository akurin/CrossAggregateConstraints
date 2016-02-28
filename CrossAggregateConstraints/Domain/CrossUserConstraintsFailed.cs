using System;
using CrossAggregateConstraints.Infrastructure.EventSourcing;

namespace CrossAggregateConstraints.Domain
{
    public sealed class CrossUserConstraintsFailed : IEvent
    {
        public Guid UserId { get; }

        public CrossUserConstraintsFailed(Guid userId)
        {
            UserId = userId;
        }
    }
}