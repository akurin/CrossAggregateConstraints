using System;

namespace CrossAggregateConstraints.Domain.Events
{
    public class UserCreated : IEvent
    {
        public UserCreated(Guid userId, UserRegistrationForm form)
        {
            if (form == null) throw new ArgumentNullException(nameof(form));

            UserId = userId;
            Form = form;
        }

        public Guid UserId { get; }
        public UserRegistrationForm Form { get; set; }
    }
}