using System;

namespace CrossAggregateValidation.Domain.Events
{
    public class UserRegistrationStarted : IEvent
    {
        public UserRegistrationStarted(Guid userId, UserRegistrationForm form)
        {
            if (form == null) throw new ArgumentNullException(nameof(form));

            UserId = userId;
            Form = form;
        }

        public Guid UserId { get; }
        public UserRegistrationForm Form { get; }
    }
}