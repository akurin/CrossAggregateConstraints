using System;
using CrossAggregateValidation.Domain;

namespace CrossAggregateValidation.Adapters.Persistance.JsonNetEventSerialization
{
    public interface IEventNamingStrategy
    {
        string GetName(IEvent @event);
        Type GetType(string name);
    }
}