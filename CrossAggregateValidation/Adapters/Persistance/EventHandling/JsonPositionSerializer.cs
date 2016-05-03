using System;
using System.Text;
using EventStore.ClientAPI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Optional;

namespace CrossAggregateValidation.Adapters.Persistance.EventHandling
{
    public class JsonPositionSerializer : IPositionSerializer
    {
        private const string EventType = "$PositionChanged";

        public EventData ToEventData(Position position)
        {
            if (position == null) throw new ArgumentNullException(nameof(position));

            var positionJObject = new JObject
            {
                {"commitPosition", position.CommitPosition},
                {"preparePosition", position.PreparePosition}
            };

            var positionBytes = ToBytes(positionJObject);

            return new EventData(
                eventId: Guid.NewGuid(),
                type: EventType,
                isJson: true,
                data: positionBytes,
                metadata: new byte[] {});
        }

        private static byte[] ToBytes(JObject jObject)
        {
            var jsonString = JsonConvert.SerializeObject(jObject);
            return Encoding.UTF8.GetBytes(jsonString);
        }

        public Option<Position> FromEventData(RecordedEvent recordedEvent)
        {
            if (recordedEvent == null) throw new ArgumentNullException(nameof(recordedEvent));

            if (recordedEvent.EventType != EventType)
                return Option.None<Position>();

            var jObject = FromBytes(recordedEvent.Data);
            var commitPosition = jObject["commitPosition"].Value<long>();
            var preparePosition = jObject["preparePosition"].Value<long>();

            var position = new Position(commitPosition: commitPosition, preparePosition: preparePosition);

            return position.Some();
        }

        private static JObject FromBytes(byte[] bytes)
        {
            var jsonString = Encoding.UTF8.GetString(bytes);
            return JsonConvert.DeserializeObject<JObject>(jsonString);
        }
    }
}