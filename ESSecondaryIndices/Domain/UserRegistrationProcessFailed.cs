using System;
using ESSecondaryIndices.Infrastructure.EventSourcing;

namespace ESSecondaryIndices.Domain
{
    public class UserRegistrationProcessFailed : IEvent
    {
        public Guid UserId { get; }

        public UserRegistrationProcessFailed(Guid userId)
        {
            UserId = userId;
        }
    }
}