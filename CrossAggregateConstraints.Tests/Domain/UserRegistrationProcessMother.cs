using System;
using CrossAggregateConstraints.Domain;
using CrossAggregateConstraints.Domain.Events;

namespace CrossAggregateConstraints.Tests.Domain
{
    public static class UserRegistrationProcessMother
    {
        public static UserRegistrationProcess InCreatedState(Guid userId)
        {
            return new UserRegistrationProcess(new[]
            {
                new UserRegistrationStarted(userId, UserRegistrationFormMother.JohnDow())
            }, addToPending: true);
        }

        public static UserRegistrationProcess InCreatingUserState(Guid userId)
        {
            return new UserRegistrationProcess(new IEvent[]
            {
                new UserRegistrationStarted(userId, UserRegistrationFormMother.JohnDow()),
                new EmailAccepted(userId)
            }, addToPending: true);
        }
    }
}