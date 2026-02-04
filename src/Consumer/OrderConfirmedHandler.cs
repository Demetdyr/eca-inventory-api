using EcaInventoryApi.Contract;
using EcaInventoryApi.Service;

namespace EcaInventoryApi.Consumer
{
	public class OrderConfirmedHandler : IMessageConsumer<OrderConfirmedEvent>
	{
		private readonly ILogger<OrderConfirmedHandler> _logger;
		private readonly IReservationService _reservationService;

		public OrderConfirmedHandler(ILogger<OrderConfirmedHandler> logger, IReservationService reservationService)
		{
			_logger = logger;
			_reservationService = reservationService;
		}

		public async Task HandleAsync(OrderConfirmedEvent message, CancellationToken cancellationToken = default)
		{
			_logger.LogInformation("Processing OrderConfirmedEvent for OrderId: {OrderId}", message.OrderId);

			await _reservationService.ConfirmReservation(message.OrderId, cancellationToken);

			_logger.LogInformation("Successfully processed OrderConfirmedEvent for OrderId: {OrderId}", message.OrderId);
		}
	}
}
