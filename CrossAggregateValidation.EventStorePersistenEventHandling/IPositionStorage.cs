using System.Threading.Tasks;
using EventStore.ClientAPI;
using Optional;

namespace ESUtils.PersistentSubscription
{
    public interface IPositionStorage
    {
        Task<Option<Position>> ReadAsync();
        Task WriteAsync(Position position);
    }
}