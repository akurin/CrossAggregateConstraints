using System;
using System.Collections.Generic;
using System.Linq;
using CrossAggregateConstraints.Domain;

namespace CrossAggregateConstraints.Tests.Application
{
    internal class FakeEventStore
    {
        private readonly object _lockObject = new object();
        private readonly List<Tuple<string, IEvent>> _events = new List<Tuple<string, IEvent>>();

        public IEnumerable<IEvent> LastSavedEvents { get; private set; }

        public FakeEventStore()
        {
            LastSavedEvents = Enumerable.Empty<IEvent>();
        }

        public SaveResult Save(string streamId, IEnumerable<IEvent> events, int expectedVersion)
        {
            if (events == null) throw new ArgumentNullException(nameof(events));

            lock (_lockObject)
            {
                var streamVersion = _events.Count(e => e.Item1 == streamId);

                if (expectedVersion != streamVersion)
                {
                    LastSavedEvents = Enumerable.Empty<IEvent>();
                    return SaveResult.WrongExpextedVersion;
                }

                var eventList = events.ToList();
                _events.AddRange(eventList.Select(e => Tuple.Create(streamId, e)));
                LastSavedEvents = eventList;
                return SaveResult.Success;
            }
        }

        public IEnumerable<IEvent> GetByStreamId(string streamId)
        {
            lock (_lockObject)
            {
                return _events
                    .Where(e => e.Item1 == streamId)
                    .Select(e => e.Item2)
                    .ToList();
            }
        }
    }
}