using EcaInventoryApi.Data;
using EcaInventoryApi.Repository.Entity;

namespace EcaInventoryApi.Repository
{
    public class ReservationRepository : IReservationRepository
    {
        private readonly ApplicationDbContext _context;

        public ReservationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<ReservationEntity>> AddReservation(List<ReservationEntity> reservationEntities)
        {
            await _context.Reservations.AddRangeAsync(reservationEntities);
            await _context.SaveChangesAsync();
            return reservationEntities;
        }
    }
}