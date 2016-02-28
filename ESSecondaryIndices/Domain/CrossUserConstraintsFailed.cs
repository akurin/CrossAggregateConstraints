using System;
using ESSecondaryIndices.Infrastructure.EventSourcing;

namespace ESSecondaryIndices.Domain
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