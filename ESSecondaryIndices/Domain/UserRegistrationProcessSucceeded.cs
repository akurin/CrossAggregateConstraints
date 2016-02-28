using System;
using ESSecondaryIndices.Infrastructure.EventSourcing;

namespace ESSecondaryIndices.Domain
{
    public class UserRegistrationProcessSucceeded : IEvent
    {
        public Guid UserId { get; }

        public UserRegistrationProcessSucceeded(Guid userId)
        {
            UserId = userId;
        }
    }
}