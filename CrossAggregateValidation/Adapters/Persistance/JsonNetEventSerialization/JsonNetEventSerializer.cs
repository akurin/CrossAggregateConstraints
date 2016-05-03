using System;
using System.Reflection;
using System.Text;
using CrossAggregateValidation.Domain;
using EventStore.ClientAPI;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Optional;

namespace CrossAggregateValidation.Adapters.Persistance.JsonNetEventSerialization
{
    public class JsonNetEventSerializer : IEventSerializer
    {
        private readonly IEventNamingStrategy _eventNamingStrategy;
        private readonly JsonSerializerSettings _serializerSettings;

        public static JsonNetEventSerializer CreateForAssembly(Assembly assembly)
        {
            if (assembly == null) throw new ArgumentNullException(nameof(assembly));

            var eventNamingStrategy = new TypeFullNameEventNamingStrategy(assembly);
            var jsonSerializerSettings = new JsonSerializerSettings();
            return new JsonNetEventSerializer(eventNamingStrategy, jsonSerializerSettings);
        }

        public JsonNetEventSerializer(
            IEventNamingStrategy eventNamingStrategy,
            JsonSerializerSettings serializerSettings)
        {
            if (eventNamingStrategy == null) throw new ArgumentNullException(nameof(eventNamingStrategy));
            if (serializerSettings == null) throw new ArgumentNullException(nameof(serializerSettings));

            _eventNamingStrategy = eventNamingStrategy;
            _serializerSettings = serializerSettings;
        }

        private static JsonSerializerSettings DefaultSerializerSettings()
        {
            return new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
        }

        public EventData ToEventData(IEvent @event)
        {
            if (@event == null) throw new ArgumentNullException(nameof(@event));

            var eventTypeName = _eventNamingStrategy.GetName(@event);
            var json = JsonConvert.SerializeObject(@event, _serializerSettings);
            var data = Encoding.UTF8.GetBytes(json);
            var metadata = new byte[0];

            return new EventData(
                eventId: Guid.NewGuid(),
                type: eventTypeName,
                isJson: true,
                data: data,
                metadata: metadata);
        }

        public Option<IEvent> FromEventData(RecordedEvent recordedEvent)
        {
            if (recordedEvent == null) throw new ArgumentNullException(nameof(recordedEvent));

            if (recordedEvent.EventType.StartsWith("$"))
                return Option.None<IEvent>();

            var json = Encoding.UTF8.GetString(recordedEvent.Data);
            var type = _eventNamingStrategy.GetType(recordedEvent.EventType);
            if (type == null)
                throw new InvalidOperationException($"Could not find type {recordedEvent.EventType}");

            var fromEventData = (IEvent) JsonConvert.DeserializeObject(json, type, _serializerSettings);

            return fromEventData == null
                ? Option.None<IEvent>()
                : Option.Some(fromEventData);
        }
    }
}