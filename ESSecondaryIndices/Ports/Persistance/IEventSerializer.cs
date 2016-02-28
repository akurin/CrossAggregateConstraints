using ESSecondaryIndices.Infrastructure.EventSourcing;
using EventStore.ClientAPI;
using Optional;

namespace ESSecondaryIndices.Ports.Persistance
{
    public interface IEventSerializer
    {
        EventData ToEventData(IEvent @event);
        Option<IEvent> FromEventData(RecordedEvent recordedEvent);
    }
}