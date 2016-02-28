using System.Collections.Generic;

namespace ESSecondaryIndices.Infrastructure.EventSourcing
{
    public interface IEventSourced
    {
        IEnumerable<IEvent> GetEvents();
    }
}