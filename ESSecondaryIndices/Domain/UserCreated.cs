using System;
using ESSecondaryIndices.Infrastructure.EventSourcing;

namespace ESSecondaryIndices.Domain
{
    public sealed class UserCreated : IEvent
    {
        public UserCreated(Guid userId, string email)
        {
            if (email == null) throw new ArgumentNullException(nameof(email));

            UserId = userId;
            Email = email;
        }

        public string Email { get; }
        public Guid UserId { get; }
    }
}