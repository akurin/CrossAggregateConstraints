using System;
using ESSecondaryIndices.Infrastructure.EventSourcing;

namespace ESSecondaryIndices.Domain
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