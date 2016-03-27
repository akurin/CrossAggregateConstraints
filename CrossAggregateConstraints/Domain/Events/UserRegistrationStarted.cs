using System;

namespace CrossAggregateConstraints.Domain.Events
{
    public class UserRegistrationStarted : IEvent
    {
        public Guid UserId { get; }
        public UserRegistrationForm Form { get; }

        public UserRegistrationStarted(Guid userId, UserRegistrationForm form)
        {
            if (form == null) throw new ArgumentNullException(nameof(form));

            UserId = userId;
            Form = form;
        }
    }
}