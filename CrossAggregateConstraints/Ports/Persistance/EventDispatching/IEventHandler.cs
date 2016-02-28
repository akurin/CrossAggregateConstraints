using System.Threading.Tasks;
using CrossAggregateConstraints.Infrastructure.EventSourcing;

namespace CrossAggregateConstraints.Ports.Persistance.EventDispatching
{
    public interface IEventHandler
    {
        Task HandleAsync(IEvent @event);
    }
}