using EcaInventoryApi.Contract;
using EcaInventoryApi.Service;

namespace EcaInventoryApi.Consumer
{
	public class OrderCreatedHandler : IMessageConsumer<OrderCreatedEvent>
	{
		private readonly ILogger<OrderCreatedHandler> _logger;

		public OrderCreatedHandler(ILogger<OrderCreatedHandler> logger)
		{
			_logger = logger;
		}

		public async Task HandleAsync(OrderCreatedEvent message, CancellationToken cancellationToken = default)
		{
			_logger.LogInformation("Processing OrderCreatedEvent for OrderId: {OrderId}", message.OrderId);

			// Handle order created event logic here

			_logger.LogInformation("Successfully processed OrderCreatedEvent for OrderId: {OrderId}", message.OrderId);

			await Task.CompletedTask;
		}
	}
}