using System.Threading.Tasks;
using EventStore.ClientAPI;
using Optional;

namespace CrossAggregateValidation.Adapters.Persistance.EventHandling
{
    public interface IPositionStorage
    {
        Task<Option<Position>> ReadAsync();
        Task WriteAsync(Position position);
    }
}