using System.Threading.Tasks;
using CrossAggregateValidation.Domain;

namespace CrossAggregateValidation.Adapters.Persistance.EventHandling
{
    public interface IEventHandler
    {
        Task HandleAsync(IEvent @event);
    }
}