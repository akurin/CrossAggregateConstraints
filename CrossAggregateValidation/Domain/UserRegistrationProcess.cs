using System;
using System.Collections.Generic;
using CrossAggregateValidation.Domain.Events;

namespace CrossAggregateValidation.Domain
{
    public class UserRegistrationProcess : IEventSourced
    {
        private readonly ICollection<IEvent> _events = new List<IEvent>();
        private UserRegistrationForm _registrationForm;

        public UserRegistrationProcess(IEnumerable<IEvent> events, bool addToPending = false)
        {
            if (events == null) throw new ArgumentNullException(nameof(events));

            foreach (var @event in events)
            {
                if (addToPending)
                {
                    Apply(@event);
                }
                else
                {
                    Mutate(@event);
                    Version++;
                }
            }
        }

        private void Apply(IEvent @event)
        {
            Mutate(@event);
            _events.Add(@event);
        }

        private void Mutate(IEvent @event)
        {
            if (@event is UserRegistrationStarted)
            {
                var created = @event as UserRegistrationStarted;
                UserId = created.UserId;
                _registrationForm = created.Form;
                State = UserRegistrationProcessState.RegistrationStarted;
            }
            else if (@event is EmailAccepted)
            {
                State = UserRegistrationProcessState.CreatingUser;
            }
            else if (@event is UserRegistrationSucceeded)
            {
                State = UserRegistrationProcessState.Succeeded;
            }
            else if (@event is UserRegistrationFailed)
            {
                State = UserRegistrationProcessState.Failed;
            }
        }

        public int Version { get; }

        public Guid UserId { get; private set; }

        public UserRegistrationProcessState State { get; private set; }

        public UserRegistrationProcess(UserRegistrationForm registrationForm)
        {
            if (registrationForm == null) throw new ArgumentNullException(nameof(registrationForm));

            var userId = Guid.NewGuid();
            Apply(new UserRegistrationStarted(userId, registrationForm));
        }

        public IEnumerable<IEvent> GetPendingEvents()
        {
            return _events;
        }

        public User CreateUser()
        {
            return new User(UserId, _registrationForm);
        }

        public void HandleEmailAccepted()
        {
            switch (State)
            {
                case UserRegistrationProcessState.RegistrationStarted:
                    Apply(new EmailAccepted(UserId));
                    break;
                case UserRegistrationProcessState.CreatingUser:
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }

        public void HandleUserCreated()
        {
            switch (State)
            {
                case UserRegistrationProcessState.CreatingUser:
                    Apply(new UserRegistrationSucceeded(UserId));
                    break;
                case UserRegistrationProcessState.Succeeded:
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }

        public void HandleEmailRejected()
        {
            switch (State)
            {
                case UserRegistrationProcessState.RegistrationStarted:
                    Apply(new UserRegistrationFailed(UserId));
                    break;
                case UserRegistrationProcessState.Failed:
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }
    }
}