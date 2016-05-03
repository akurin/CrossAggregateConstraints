using EventStore.ClientAPI;
using Optional;

namespace CrossAggregateValidation.Adapters.Persistance.EventHandling
{
    public interface IPositionSerializer
    {
        EventData ToEventData(Position position);
        Option<Position> FromEventData(RecordedEvent recored);
    }
}