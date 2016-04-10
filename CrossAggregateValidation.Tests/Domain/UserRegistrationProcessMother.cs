using System;
using CrossAggregateValidation.Domain;
using CrossAggregateValidation.Domain.Events;

namespace CrossAggregateValidation.Tests.Domain
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