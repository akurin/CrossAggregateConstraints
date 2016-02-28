using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using CrossAggregateConstraints.Domain;

namespace CrossAggregateConstraints.Ports.Costraints
{
    public sealed class InMemoryCrossAggregateConstraints : ICrossAggregateConstraints
    {
        private readonly ConcurrentDictionary<string, Guid> _dic = new ConcurrentDictionary<string, Guid>();

        public Task<bool> AddAsync(Guid userId, UserRegistrationForm registrationForm)
        {
            if (registrationForm == null) throw new ArgumentNullException(nameof(registrationForm));
            if (registrationForm.Email == null)
                throw new ArgumentException("Email should not be null", nameof(registrationForm));

            var tryAddResult = _dic.TryAdd(registrationForm.Email, userId);
            return Task.FromResult(tryAddResult);
        }
    }
}