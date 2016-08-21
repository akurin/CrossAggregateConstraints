using System;
using System.Threading.Tasks;
using CrossAggregateValidation.Application.EventHandling;
using CrossAggregateValidation.Domain;
using CrossAggregateValidation.Domain.Events;
using EventStore.ClientAPI;
using Optional.Unsafe;

namespace CrossAggregateValidation.Adapters.Persistance.EventHandling
{
    public class UserRegistrationEventHandlerAdapter
    {
        private readonly IEventSerializer _eventSerializer;
        private readonly UserRegistrationEventHandler _userRegistrationEventHandler;

        public UserRegistrationEventHandlerAdapter(
            IEventSerializer eventSerializer,
            UserRegistrationEventHandler userRegistrationEventHandler)
        {
            if (eventSerializer == null) throw new ArgumentNullException(nameof(eventSerializer));
            if (userRegistrationEventHandler == null)
                throw new ArgumentNullException(nameof(userRegistrationEventHandler));

            _eventSerializer = eventSerializer;
            _userRegistrationEventHandler = userRegistrationEventHandler;
        }

        public async Task HandleAsync(ResolvedEvent resolvedEvent)
        {
            var maybeEvent = _eventSerializer.FromEventData(resolvedEvent.Event);
            if (!maybeEvent.HasValue)
                return;

            await CallHandlerAsync(maybeEvent.ValueOrFailure());
        }

        private async Task CallHandlerAsync(IEvent @event)
        {
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