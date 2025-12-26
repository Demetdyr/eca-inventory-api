using EcaIncentoryApi.Contract;
using EcaIncentoryApi.Service;

namespace EcaIncentoryApi.Consumer
{
	public class OrderCreatedHandler : IMessageConsumer<OrderCreatedEvent>
	{
		private readonly ILogger<OrderCreatedHandler> _logger;
		private readonly IReservationService _reservationService;

		public OrderCreatedHandler(ILogger<OrderCreatedHandler> logger, IReservationService reservationService)
		{
			_logger = logger;
			_reservationService = reservationService;
		}

		public async Task HandleAsync(OrderCreatedEvent message, CancellationToken cancellationToken = default)
		{
			_logger.LogInformation("Processing OrderCreatedEvent for OrderId: {OrderId}", message.OrderId);

			await _reservationService.CreateReservation(message);

			_logger.LogInformation("Successfully processed OrderCreatedEvent for OrderId: {OrderId}", message.OrderId);
		}
	}
}