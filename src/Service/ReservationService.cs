using EcaIncentoryApi.Contract;
using EcaIncentoryApi.Service;
using EcaInventoryApi.Data;
using EcaInventoryApi.Model;
using EcaInventoryApi.Repository;
using EcaInventoryApi.Repository.Entity;
using EcaOrderApi.Contract;
using EcaOrderApi.Messaging;

namespace EcaInventoryApi.Service
{
    public class ReservationService : IReservationService
    {
		private readonly IStockItemRepository _stockItemRepository;
		private readonly ApplicationDbContext _context;
		private readonly IReservationRepository _reservationRepository;
		private readonly ILogger<ReservationService> _logger;
		private readonly IMessagePublisher _messagePublisher;

		public ReservationService(IStockItemRepository stockItemRepository, ApplicationDbContext context, IReservationRepository reservationRepository, ILogger<ReservationService> logger, IMessagePublisher messagePublisher)
		{
			_stockItemRepository = stockItemRepository;
			_context = context;
			_reservationRepository = reservationRepository;
			_logger = logger;
			_messagePublisher = messagePublisher;
		}

		public async Task CreateReservation(OrderCreatedEvent message)
		{

			if (message.Items.Count == 1)
			{
				var reservationMessage = new OrderStockReservedEvent
				{
					OrderId = message.OrderId,
					Items = message.Items.Select(i => new OrderStockReservedEventItem
					{
						ProductSku = i.ProductSku!,
						Quantity = i.Quantity
					}).ToList()
				};
				await _messagePublisher.PublishAsync("inventory.order.stock.reserved", reservationMessage);
			}
			else
			{
				var reservationMessage = new OrderStockRejectedEvent
				{
					OrderId = message.OrderId,
					Reason = "Stock not available",
					Items = message.Items.Select(i => new OrderStockRejectedEventItem
					{
						ProductSku = i.ProductSku!,
						Quantity = i.Quantity
					}).ToList()
				};
				await _messagePublisher.PublishAsync("inventory.order.stock.rejected", reservationMessage);
			}
		}
    }
}