using System;
using ESSecondaryIndices.Infrastructure.EventSourcing;

namespace ESSecondaryIndices.Domain
{
    public class UserRegistrationProcessStarted : IEvent
    {
        public Guid UserId { get; }
        public UserRegistrationForm Form { get; }

        public UserRegistrationProcessStarted(Guid userId, UserRegistrationForm form)
        {
            UserId = userId;
            Form = form;
        }
    }
}