using EcaInventoryApi.Repository.Entity;

namespace EcaInventoryApi.Repository
{
    public interface IReservationRepository
    {
        Task<List<ReservationEntity>> AddReservation(List<ReservationEntity> reservationEntities);
    }
}