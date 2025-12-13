using EcaIncentoryApi.Service;
using EcaInventoryApi.Contracts;
using EcaInventoryApi.Data;
using EcaInventoryApi.Model;
using EcaInventoryApi.Publisher;
using EcaInventoryApi.Repository;
using EcaInventoryApi.Repository.Entity;

namespace EcaInventoryApi.Service
{
    public class ReservationService : IReservationService
    {
        private readonly IStockItemRepository _stockItemRepository;
        private readonly IRabbitMqPublisher _publisher;
        private readonly ApplicationDbContext _context;
        private readonly IReservationRepository _reservationRepository;

        public ReservationService(IStockItemRepository stockItemRepository, IRabbitMqPublisher publisher, ApplicationDbContext context, IReservationRepository reservationRepository)
        {
            _stockItemRepository = stockItemRepository;
            _publisher = publisher;
            _context = context;
            _reservationRepository = reservationRepository;
        }

        public async Task CreateReservation(OrderCreatedEvent message)
        {
            try
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

                var stockItems = await _stockItemRepository.GetAllByProductSkuAsync(
                    message.Items.Select(i => i.Sku).ToList()
                );
                var list = new List<Repository.Entity.ReservationEntity>();

                foreach (var item in message.Items)
                {
                    var stock = stockItems.First(x => x.ProductSku == item.Sku);

                    if (stock.Quantity - stock.ReservedQuantity < item.Quantity)
                    {
                        await transaction.RollbackAsync();

                        var rejected = new StockRejectedEvent
                        {
                            OrderId = message.OrderId,
                            Items = message.Items,
                            Reason = $"Not enough stock for SKU {item.Sku}"
                        };

                        await _publisher.PublishAsync(rejected, "stock.rejected");
                        return;
                    }

                    stock.ReservedQuantity += item.Quantity;
                    var reservationEntity = new ReservationEntity
                    {
                        OrderId = message.OrderId,
                        ProductSku = item.Sku,
                        Quantity = item.Quantity,
                        Status = ReservationStatus.Confirmed,
                        ExpiresAt = DateTime.UtcNow.AddMinutes(30),
                    };
                    list.Add(reservationEntity);
                }

                await _reservationRepository.AddReservation(list);

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                var rejected = new StockRejectedEvent
                {
                    OrderId = message.OrderId,
                    Items = message.Items,
                    Reason = ex.Message
                };
                System.Console.WriteLine($"Reservation failed: {ex.Message}");
                await _publisher.PublishAsync(rejected, "stock.rejected");
                return;
            }

            var reserved = new StockReservedEvent
            {
                OrderId = message.OrderId,
                Items = message.Items
            };

            await _publisher.PublishAsync(reserved, "stock.reserved");
        }
    }
}