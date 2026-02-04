using EcaInventoryApi.Data;
using EcaInventoryApi.Repository.Entity;
using Microsoft.EntityFrameworkCore;

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

        public async Task<List<ReservationEntity>> GetByOrderIdAsync(int orderId, CancellationToken cancellationToken = default)
        {
            return await _context.Reservations
                .Where(r => r.OrderId == orderId)
                .ToListAsync(cancellationToken);
        }
    }
}