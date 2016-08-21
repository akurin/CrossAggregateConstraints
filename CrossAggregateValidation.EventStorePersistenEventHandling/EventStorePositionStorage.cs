using System;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using Optional;

namespace ESUtils.PersistentSubscription
{
    public class EventStorePositionStorage : IPositionStorage
    {
        private readonly IEventStoreConnection _connection;
        private readonly string _stream;
        private readonly IPositionSerializer _serializer;

        public EventStorePositionStorage(IEventStoreConnection connection, string stream, IPositionSerializer serializer)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            if (serializer == null) throw new ArgumentNullException(nameof(serializer));

            _connection = connection;
            _stream = stream;
            _serializer = serializer;
        }

        public async Task<Option<Position>> ReadAsync()
        {
            var readResult = await _connection.ReadStreamEventsBackwardAsync(
                stream: _stream,
                start: StreamPosition.End,
                count: 1,
                resolveLinkTos: false);

            if (readResult.Events.Length == 0)
                return Option.None<Position>();

            var resolvedEvent = readResult.Events[0];
            return _serializer.FromEventData(resolvedEvent.OriginalEvent);
        }

        public async Task WriteAsync(Position position)
        {
            var eventData = _serializer.ToEventData(position);

            await _connection.AppendToStreamAsync(
                stream: _stream,
                expectedVersion: ExpectedVersion.Any,
                events: eventData);
        }
    }
}
