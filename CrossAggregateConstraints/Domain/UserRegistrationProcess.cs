using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CrossAggregateConstraints.Infrastructure.EventSourcing;

namespace CrossAggregateConstraints.Domain
{
    public sealed class UserRegistrationProcess : IEventSourced
    {
        private readonly ICrossAggregateConstraints _crossAggregateConstraints;
        private readonly ICollection<IEvent> _events = new List<IEvent>();

        public Guid UserId { get; private set; }
        public UserRegistrationForm RegistrationForm { private set; get; }

        public UserRegistrationProcessState State { get; private set; }

        public UserRegistrationProcess(
            UserRegistrationForm registrationForm,
            ICrossAggregateConstraints crossAggregateConstraints)
        {
            if (crossAggregateConstraints == null) throw new ArgumentNullException(nameof(crossAggregateConstraints));

            _crossAggregateConstraints = crossAggregateConstraints;

            var userId = Guid.NewGuid();
            Apply(new UserRegistrationProcessStarted(userId, registrationForm));
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

        private void When(CrossUserConstraintsAdded e)
        {
            
        }

        private void When(CrossUserConstraintsFailed e)
        {
            
        }

        private void When(UserRegistrationProcessStarted @event)
        {
            UserId = @event.UserId;
            RegistrationForm = @event.Form;
            State = UserRegistrationProcessState.InProgress;
        }

        private void When(UserRegistrationProcessSucceeded @event)
        {
            State = UserRegistrationProcessState.Succeeded;
        }

        private void When(UserRegistrationProcessFailed @event)
        {
            State = UserRegistrationProcessState.Failed;
        }

        public UserRegistrationProcess(IEnumerable<IEvent> events, ICrossAggregateConstraints crossAggregateConstraints)
        {
            if (events == null) throw new ArgumentNullException(nameof(events));
            if (crossAggregateConstraints == null) throw new ArgumentNullException(nameof(crossAggregateConstraints));

            _crossAggregateConstraints = crossAggregateConstraints;

            foreach (var @event in events)
            {
                Mutate(@event);
            }
        }

        public IEnumerable<IEvent> GetEvents()
        {
            return _events;
        }

        public async Task AddCrossUserConstraints()
        {
            var constraintsAdded = await _crossAggregateConstraints.AddAsync(UserId, RegistrationForm);
            if (constraintsAdded)
            {
                Apply(new CrossUserConstraintsAdded(UserId));
            }
            else
            {
                Apply(new CrossUserConstraintsFailed(UserId));
            }
        }

        public void Succeed()
        {
            Apply(new UserRegistrationProcessSucceeded(UserId));
        }

        public void Fail()
        {
            Apply(new UserRegistrationProcessFailed(UserId));
        }
    }
}