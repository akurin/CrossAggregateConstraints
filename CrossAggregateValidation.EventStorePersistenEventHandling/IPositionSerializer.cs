using EventStore.ClientAPI;
using Optional;

namespace ESUtils.PersistentSubscription
{
    public interface IPositionSerializer
    {
        EventData ToEventData(Position position);
        Option<Position> FromEventData(RecordedEvent recored);
    }
}