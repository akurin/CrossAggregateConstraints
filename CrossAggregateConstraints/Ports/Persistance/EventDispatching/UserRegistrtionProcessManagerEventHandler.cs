using System;
using System.Threading.Tasks;
using CrossAggregateConstraints.Application;
using CrossAggregateConstraints.Domain;
using CrossAggregateConstraints.Infrastructure.EventSourcing;

namespace CrossAggregateConstraints.Ports.Persistance.EventDispatching
{
    public sealed class UserRegistrtionProcessManagerEventHandler : IEventHandler
    {
        private readonly UserRegistrationProcessManager _userRegistrationProcessManager;

        public UserRegistrtionProcessManagerEventHandler(
            UserRegistrationProcessManager userRegistrationProcessManager)
        {
            if (userRegistrationProcessManager == null)
                throw new ArgumentNullException(nameof(userRegistrationProcessManager));

            _userRegistrationProcessManager = userRegistrationProcessManager;
        }

        public async Task HandleAsync(IEvent @event)
        {
            if (@event == null) throw new ArgumentNullException(nameof(@event));

            if (@event is UserRegistrationProcessStarted)
            {
                await _userRegistrationProcessManager.HandleAsync(@event as UserRegistrationProcessStarted);
            }
            else if (@event is CrossUserConstraintsAdded)
            {
                await _userRegistrationProcessManager.HandleAsync(@event as CrossUserConstraintsAdded);
            }
            else if (@event is CrossUserConstraintsFailed)
            {
                await _userRegistrationProcessManager.HandleAsync(@event as CrossUserConstraintsFailed);
            }
            else if (@event is UserCreated)
            {
                await _userRegistrationProcessManager.HandleAsync(@event as UserCreated);
            }
        }
    }
}