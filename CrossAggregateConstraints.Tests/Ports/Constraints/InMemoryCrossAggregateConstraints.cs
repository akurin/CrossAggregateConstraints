using System;
using CrossAggregateConstraints.Domain;
using CrossAggregateConstraints.Ports.Costraints;
using NSpec;

namespace CrossAggregateConstraints.Tests.Ports.Constraints
{
    public sealed class InMemoryCrossAggregateConstraintsTest : nspec
    {
        private void before_each()
        {
            _sut = new InMemoryCrossAggregateConstraints();
        }

        private void describe_AddAsync()
        {
            context["when constraint is not present"] = () =>
            {
                var result = true;

                before = () =>
                {
                    var userId = Guid.NewGuid();
                    result = _sut.AddAsync(userId, new UserRegistrationForm("a@example.com")).Result;
                };

                it["returns true"] = () => result.is_true();
            };

            context["when constraint is present"] = () =>
            {
                var result = true;

                before = () =>
                {
                    var user1Id = Guid.NewGuid();
                    _sut.AddAsync(user1Id, new UserRegistrationForm("a@example.com")).Wait();

                    var user2Id = Guid.NewGuid();
                    result = _sut.AddAsync(user2Id, new UserRegistrationForm("a@example.com")).Result;
                };

                it["returns false"] = () => result.is_false();
            };
        }

        private ICrossAggregateConstraints _sut;
    }
}