using CrossAggregateConstraints.Domain;
using EventStore.ClientAPI;
using Optional;

namespace CrossAggregateConstraints.Ports.Persistance
{
    public interface IEventSerializer
    {
        EventData ToEventData(IEvent @event);
        Option<IEvent> FromEventData(RecordedEvent recordedEvent);
    }
}