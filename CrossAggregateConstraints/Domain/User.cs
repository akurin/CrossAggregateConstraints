using System;
using System.Collections.Generic;
using CrossAggregateConstraints.Domain.Events;

namespace CrossAggregateConstraints.Domain
{
    public class User : IEventSourced
    {
        private readonly ICollection<IEvent> _events = new List<IEvent>();

        public User(IEnumerable<IEvent> events)
        {
            if (events == null) throw new ArgumentNullException(nameof(events));

            foreach (var @event in events)
            {
                Mutate(@event);
                Version++;
            }
        }

        private void Mutate(IEvent @event)
        {
            if (@event is UserCreated)
            {
                var created = @event as UserCreated;
                Id = created.UserId;
            }
        }

        public Guid Id { get; private set; }

        public int Version { get; }

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

        public IEnumerable<IEvent> GetPendingEvents()
        {
            return _events;
        }
    }
}