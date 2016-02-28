using CrossAggregateConstraints.Infrastructure.EventSourcing;
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