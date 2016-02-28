using System;
using System.Collections.Generic;
using ESSecondaryIndices.Infrastructure.EventSourcing;

namespace ESSecondaryIndices.Domain
{
    public sealed class User : IEventSourced
    {
        private readonly ICollection<IEvent> _events = new List<IEvent>();

        public Guid Id { get; private set; }
        public string Email { get; private set; }

        public User(IEnumerable<IEvent> events)
        {
            if (events == null) throw new ArgumentNullException(nameof(events));

            foreach (var @event in events)
            {
                Mutate(@event);
            }
        }

        public User(Guid id, string email)
        {
            if (email == null) throw new ArgumentNullException(nameof(email));

            Apply(new UserCreated(id, email));
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
            Email = @event.Email;
        }

        public IEnumerable<IEvent> GetEvents()
        {
            return _events;
        }
    }
}