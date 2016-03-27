using System.Threading.Tasks;
using CrossAggregateConstraints.Infrastructure.EventSourcing;

namespace CrossAggregateConstraints.Ports.Persistance.EventHandling
{
    public interface IEventHandler
    {
        Task HandleAsync(IEvent @event);
    }
}