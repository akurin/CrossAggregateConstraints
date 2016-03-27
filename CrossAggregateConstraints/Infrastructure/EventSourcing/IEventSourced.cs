using System.Collections.Generic;

namespace CrossAggregateConstraints.Infrastructure.EventSourcing
{
    public interface IEventSourced
    {
        int Version { get; }
        IEnumerable<IEvent> GetEvents();
    }
}