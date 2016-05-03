using System;
using System.Reflection;
using CrossAggregateValidation.Domain;

namespace CrossAggregateValidation.Adapters.Persistance.JsonNetEventSerialization
{
    public class TypeFullNameEventNamingStrategy : IEventNamingStrategy
    {
        private readonly Assembly _assembly;

        public TypeFullNameEventNamingStrategy(Assembly assembly)
        {
            if (assembly == null) throw new ArgumentNullException(nameof(assembly));

            _assembly = assembly;
        }

        public string GetName(IEvent @event)
        {
            if (@event == null) throw new ArgumentNullException(nameof(@event));

            return @event.GetType().FullName;
        }

        public Type GetType(string evenTypeName)
        {
            if (evenTypeName == null) throw new ArgumentNullException(nameof(evenTypeName));

            return _assembly.GetType(evenTypeName);
        }
    }
}