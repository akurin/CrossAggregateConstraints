using System;
using CrossAggregateConstraints.Domain;
using CrossAggregateConstraints.Ports.Constraints;
using NSpec;

namespace CrossAggregateConstraints.Tests.Ports.Constraints
{
    public class UseryByEmailInMemoryIndexSpec : nspec
    {
        private UserByEmailInMemoryIndex _sut;
        private IndexResult _result;

        private void before_each()
        {
            _sut = new UserByEmailInMemoryIndex();
        }

        private void describe_AddAsync()
        {
            var userId = Guid.NewGuid();
            const string email = "johndow@example.com";

            context["when index does not contain email"] = () =>
            {
                before = () => _result = _sut.AddAsync(email, userId).Result;

                it["returns EmailAccepted"] = () => _result.should_be(IndexResult.EmailAccepted);
                it["is idempotent"] = () =>
                {
                    _result = _sut.AddAsync(email, userId).Result;
                    _result.should_be(IndexResult.EmailAccepted);
                };
            };

            context["when index contains email associated with another user"] = () =>
            {
                before = () =>
                {
                    _sut.AddAsync(email, userId).Wait();
                    _result = _sut.AddAsync(email, Guid.NewGuid()).Result;
                };

                it["returns EmailAccepted"] = () => _result.should_be(IndexResult.EmailRejected);
            };
        }
    }
}