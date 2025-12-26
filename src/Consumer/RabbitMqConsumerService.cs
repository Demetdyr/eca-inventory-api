using System.Text.Json;
using EcaOrderApi.Messaging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace EcaIncentoryApi.Consumer
{
	public class RabbitMqConsumerService<TMessage, THandler> : BackgroundService
		where TMessage : class
		where THandler : IMessageConsumer<TMessage>
	{
		private readonly IRabbitMqConnectionFactory _connectionFactory;
		private readonly IServiceScopeFactory _scopeFactory;
		private readonly ILogger<RabbitMqConsumerService<TMessage, THandler>> _logger;
		private readonly string _queueName;
		private readonly JsonSerializerOptions _jsonOptions;

		private IChannel? _channel;

		public RabbitMqConsumerService(
			IRabbitMqConnectionFactory connectionFactory,
			IServiceScopeFactory scopeFactory,
			ILogger<RabbitMqConsumerService<TMessage, THandler>> logger,
			string queueName)
		{
			_connectionFactory = connectionFactory;
			_scopeFactory = scopeFactory;
			_logger = logger;
			_queueName = queueName;
			_jsonOptions = new JsonSerializerOptions
			{
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase
			};
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			_logger.LogInformation("Starting consumer for queue {QueueName}", _queueName);

			try
			{
				var connection = await _connectionFactory.GetConnectionAsync(stoppingToken);
				_channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

				// Declare queue (idempotent)
				await _channel.QueueDeclareAsync(
					queue: _queueName,
					durable: true,
					exclusive: false,
					autoDelete: false,
					arguments: null,
					cancellationToken: stoppingToken);

				// Prefetch 1 message at a time
				await _channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false, cancellationToken: stoppingToken);

				var consumer = new AsyncEventingBasicConsumer(_channel);

				consumer.ReceivedAsync += async (_, eventArgs) =>
				{
					await ProcessMessageAsync(eventArgs, stoppingToken);
				};

				await _channel.BasicConsumeAsync(
					queue: _queueName,
					autoAck: false,
					consumer: consumer,
					cancellationToken: stoppingToken);

				_logger.LogInformation("Consumer started for queue {QueueName}", _queueName);

				// Keep the service running
				await Task.Delay(Timeout.Infinite, stoppingToken);
			}
			catch (OperationCanceledException)
			{
				_logger.LogInformation("Consumer for queue {QueueName} is stopping", _queueName);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error in consumer for queue {QueueName}", _queueName);
				throw;
			}
		}

		private async Task ProcessMessageAsync(BasicDeliverEventArgs eventArgs, CancellationToken cancellationToken)
		{
			var messageId = eventArgs.BasicProperties?.MessageId ?? "unknown";

			try
			{
				_logger.LogInformation(
					"Received message {MessageId} from queue {QueueName}",
					messageId,
					_queueName);

				var message = JsonSerializer.Deserialize<TMessage>(eventArgs.Body.Span, _jsonOptions);

				if (message is null)
				{
					_logger.LogWarning("Failed to deserialize message {MessageId}", messageId);
					await _channel!.BasicNackAsync(eventArgs.DeliveryTag, multiple: false, requeue: false, cancellationToken);
					return;
				}

				// Create a scope to resolve scoped services
				using var scope = _scopeFactory.CreateScope();
				var handler = scope.ServiceProvider.GetRequiredService<THandler>();

				await handler.HandleAsync(message, cancellationToken);

				// Acknowledge successful processing
				await _channel!.BasicAckAsync(eventArgs.DeliveryTag, multiple: false, cancellationToken);

				_logger.LogInformation(
					"Successfully processed message {MessageId} from queue {QueueName}",
					messageId,
					_queueName);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error processing message {MessageId} from queue {QueueName}", messageId, _queueName);

				// Requeue the message for retry
				await _channel!.BasicNackAsync(eventArgs.DeliveryTag, multiple: false, requeue: true, cancellationToken);
			}
		}

		public override async Task StopAsync(CancellationToken cancellationToken)
		{
			_logger.LogInformation("Stopping consumer for queue {QueueName}", _queueName);

			if (_channel is not null)
			{
				await _channel.CloseAsync(cancellationToken);
				_channel.Dispose();
			}

			await base.StopAsync(cancellationToken);
		}
	}
}