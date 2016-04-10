using System.Collections.Generic;

namespace CrossAggregateValidation.Domain
{
    public interface IEventSourced
    {
        int Version { get; }
        IEnumerable<IEvent> GetPendingEvents();
    }
}