using System.Collections.Generic;

namespace CrossAggregateConstraints.Domain
{
    public interface IEventSourced
    {
        int Version { get; }
        IEnumerable<IEvent> GetEvents();
    }
}