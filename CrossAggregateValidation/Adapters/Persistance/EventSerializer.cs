using System;
using System.Text;
using CrossAggregateValidation.Domain;
using EventStore.ClientAPI;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Optional;

namespace CrossAggregateValidation.Adapters.Persistance
{
    public class EventSerializer : IEventSerializer
    {
        public EventData ToEventData(IEvent @event)
        {
            if (@event == null) throw new ArgumentNullException(nameof(@event));

            var settings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            var json = JsonConvert.SerializeObject(@event, settings);
            var data = Encoding.UTF8.GetBytes(json);
            var metadata = new byte[0];

            return new EventData(
                eventId: Guid.NewGuid(),
                type: @event.GetType().FullName,
                isJson: true,
                data: data,
                metadata: metadata);
        }

        public Option<IEvent> FromEventData(RecordedEvent recordedEvent)
        {
            if (recordedEvent == null) throw new ArgumentNullException(nameof(recordedEvent));

            var settings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            if (recordedEvent.EventType.StartsWith("$"))
                return Option.None<IEvent>();

            var json = Encoding.UTF8.GetString(recordedEvent.Data);

            var fromEventData = (IEvent) JsonConvert.DeserializeObject(json, Type.GetType(recordedEvent.EventType), settings);
            return fromEventData == null
                ? Option.None<IEvent>()
                : Option.Some(fromEventData);
        }
    }
}