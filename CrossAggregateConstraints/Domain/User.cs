using System;
using System.Collections.Generic;
using CrossAggregateConstraints.Domain.Events;

namespace CrossAggregateConstraints.Domain
{
    public class User : IEventSourced
    {
        private readonly ICollection<IEvent> _events = new List<IEvent>();

        public Guid Id { get; private set; }
        public int Version { get; private set; }

        public User(IEnumerable<IEvent> events)
        {
            if (events == null) throw new ArgumentNullException(nameof(events));

            foreach (var @event in events)
            {
                Mutate(@event);
                Version++;
            }
        }

        public User(Guid id, UserRegistrationForm form)
        {
            if (form == null) throw new ArgumentNullException(nameof(form));

            Apply(new UserCreated(id, form));
        }

        private void Apply(IEvent @event)
        {
            Mutate(@event);
            _events.Add(@event);
        }

        private void Mutate(IEvent @event)
        {
            When((dynamic) @event);
        }

        private void When(UserCreated @event)
        {
            Id = @event.UserId;
        }

        public IEnumerable<IEvent> GetEvents()
        {
            return _events;
        }
    }
}