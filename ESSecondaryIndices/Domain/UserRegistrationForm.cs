namespace ESSecondaryIndices.Domain
{
    public sealed class UserRegistrationForm
    {
        public string Email { get; }

        public UserRegistrationForm(string email)
        {
            Email = email;
        }
    }
}