using System.Threading.Tasks;
using ESSecondaryIndices.Infrastructure.EventSourcing;

namespace ESSecondaryIndices.Ports.Persistance.EventDispatching
{
    public interface IEventHandler
    {
        Task HandleAsync(IEvent @event);
    }
}