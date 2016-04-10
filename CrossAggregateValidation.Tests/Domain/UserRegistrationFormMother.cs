using CrossAggregateValidation.Domain;

namespace CrossAggregateValidation.Tests.Domain
{
    internal static class UserRegistrationFormMother
    {
        public static UserRegistrationForm JohnDow()
        {
            return new UserRegistrationForm("john@example.com");
        }
    }
}
