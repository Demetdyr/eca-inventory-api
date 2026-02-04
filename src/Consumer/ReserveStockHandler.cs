using EcaInventoryApi.Contract;
using EcaInventoryApi.Service;

namespace EcaInventoryApi.Consumer
{
	public class ReserveStockHandler : IMessageConsumer<ReserveStockCommand>
	{
		private readonly ILogger<ReserveStockHandler> _logger;
		private readonly IReservationService _reservationService;

		public ReserveStockHandler(ILogger<ReserveStockHandler> logger, IReservationService reservationService)
		{
			_logger = logger;
			_reservationService = reservationService;
		}

		public async Task HandleAsync(ReserveStockCommand message, CancellationToken cancellationToken = default)
		{
			_logger.LogInformation("Processing ReserveStockCommand for OrderId: {OrderId}", message.OrderId);

			await _reservationService.CreateReservation(message);

			_logger.LogInformation("Successfully processed ReserveStockCommand for OrderId: {OrderId}", message.OrderId);
		}
	}
}