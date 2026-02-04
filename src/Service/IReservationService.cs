using EcaInventoryApi.Contract;
using EcaInventoryApi.Model;
using EcaInventoryApi.Repository.Entity;

namespace EcaInventoryApi.Service
{
    public interface IReservationService
    {
		Task CreateReservation(ReserveStockCommand message);
		Task ConfirmReservation(int orderId, CancellationToken cancellationToken = default);
    }
}