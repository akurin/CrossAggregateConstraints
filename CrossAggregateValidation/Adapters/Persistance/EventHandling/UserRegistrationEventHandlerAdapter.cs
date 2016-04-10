using System;
using System.Threading.Tasks;
using CrossAggregateValidation.Application.EventHandling;
using CrossAggregateValidation.Domain;
using CrossAggregateValidation.Domain.Events;

namespace CrossAggregateValidation.Adapters.Persistance.EventHandling
{
    public class UserRegistrationEventHandlerAdapter : IEventHandler
    {
        private readonly UserRegistrationEventHandler _userRegistrationEventHandler;

        public UserRegistrationEventHandlerAdapter(
            UserRegistrationEventHandler userRegistrationEventHandler)
        {
            if (userRegistrationEventHandler == null)
                throw new ArgumentNullException(nameof(userRegistrationEventHandler));

            _userRegistrationEventHandler = userRegistrationEventHandler;
        }

        public async Task HandleAsync(IEvent @event)
        {
            if (@event == null) throw new ArgumentNullException(nameof(@event));

            if (@event is UserRegistrationStarted)
            {
                await _userRegistrationEventHandler.HandleAsync(@event as UserRegistrationStarted);
            }
            else if (@event is EmailAccepted)
            {
                await _userRegistrationEventHandler.HandleAsync(@event as EmailAccepted);
            }
            else if (@event is UserCreated)
            {
                await _userRegistrationEventHandler.HandleAsync(@event as UserCreated);
            }
        }
    }
}