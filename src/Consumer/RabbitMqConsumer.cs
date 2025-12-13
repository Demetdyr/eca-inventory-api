
using System.Text;
using System.Text.Json;
using EcaIncentoryApi.Service;
using EcaInventoryApi.Contracts;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace EcaInventoryApi.Consumer
{
    public class OrderCreatedConsumer : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConnection _connection;

        public OrderCreatedConsumer(IServiceScopeFactory scopeFactory, IConnection connection)
        {
            _scopeFactory = scopeFactory;
            _connection = connection;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var channel = await _connection.CreateChannelAsync();

            await channel.QueueDeclareAsync("order.reserve", false, false, false);

            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.ReceivedAsync += async (sender, args) =>
            {
                using var scope = _scopeFactory.CreateScope();

                var reservationService = scope.ServiceProvider.GetRequiredService<IReservationService>();

                var json = Encoding.UTF8.GetString(args.Body.ToArray());
                var message = JsonSerializer.Deserialize<OrderCreatedEvent>(json);

                if (message != null)
                {
                    await reservationService.CreateReservation(message);
                }

                await channel.BasicAckAsync(args.DeliveryTag, false);
            };

            await channel.BasicConsumeAsync("order.reserve", false, consumer);
        }
    }
}
