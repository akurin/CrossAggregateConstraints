using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CrossAggregateConstraints.Domain;

namespace CrossAggregateConstraints.Ports.Constraints
{
    public class UserByEmailInMemoryIndex : IUserByEmailIndex
    {
        private readonly Dictionary<string, Guid> _dic = new Dictionary<string, Guid>();
        private readonly object _lockObject = new object();

        public Task<IndexResult> AddAsync(string email, Guid userId)
        {
            if (email == null) throw new ArgumentNullException(nameof(email));

            lock (_lockObject)
            {
                if (_dic.ContainsKey(email))
                {
                    var result = _dic[email] == userId
                        ? IndexResult.EmailAccepted
                        : IndexResult.EmailRejected;

                    return Task.FromResult(result);
                }

                _dic.Add(email, userId);
                return Task.FromResult(IndexResult.EmailAccepted);
            }
        }
    }
}