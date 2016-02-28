using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EventStore.ClientAPI;

namespace CrossAggregateConstraints.Ports.Persistance.Repositories
{
    internal static class EventStoreConnectionExtensions
    {
        private const int EventSliceLength = 500;

        public static async Task<IEnumerable<ResolvedEvent>> ReadSreamEventsAsync(this IEventStoreConnection connection,
            string stream)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));

            StreamEventsSlice slice;
            var start = 0;
            var result = new List<ResolvedEvent>();

            do
            {
                slice = await connection.ReadStreamEventsForwardAsync(
                    stream: stream,
                    start: start,
                    resolveLinkTos: false,
                    count: EventSliceLength);

                result.AddRange(slice.Events);

            } while (!slice.IsEndOfStream);

            return result;
        }
    }
}