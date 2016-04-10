using CrossAggregateValidation.Domain;
using EventStore.ClientAPI;
using Optional;

namespace CrossAggregateValidation.Adapters.Persistance
{
    public interface IEventSerializer
    {
        EventData ToEventData(IEvent @event);
        Option<IEvent> FromEventData(RecordedEvent recordedEvent);
    }
}