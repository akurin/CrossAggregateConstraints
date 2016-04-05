using System;

namespace CrossAggregateConstraints.Domain
{
    public class UserRegistrationForm
    {
        public UserRegistrationForm(string email)
        {
            if (email == null) throw new ArgumentNullException(nameof(email));

            Email = email;
        }

        public string Email { get; }
    }
}