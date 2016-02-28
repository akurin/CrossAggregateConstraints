using System.Collections.Generic;

namespace CrossAggregateConstraints.Infrastructure.EventSourcing
{
    public interface IEventSourced
    {
        IEnumerable<IEvent> GetEvents();
    }
}