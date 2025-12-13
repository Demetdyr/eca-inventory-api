using EcaInventoryApi.Contracts;
using EcaInventoryApi.Model;
using EcaInventoryApi.Repository.Entity;

namespace EcaIncentoryApi.Service
{
    public interface IReservationService
    {
        Task CreateReservation(OrderCreatedEvent message);
    }
}