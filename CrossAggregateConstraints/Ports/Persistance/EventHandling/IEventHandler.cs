using System.Threading.Tasks;
using CrossAggregateConstraints.Domain;

namespace CrossAggregateConstraints.Ports.Persistance.EventHandling
{
    public interface IEventHandler
    {
        Task HandleAsync(IEvent @event);
    }
}