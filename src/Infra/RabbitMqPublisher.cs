using System.Text.Json;
using RabbitMQ.Client;

namespace EcaOrderApi.Messaging
{
	public class RabbitMqPublisher : IMessagePublisher
	{
		private readonly IRabbitMqConnectionFactory _connectionFactory;
		private readonly ILogger<RabbitMqPublisher> _logger;
		private readonly JsonSerializerOptions _jsonOptions;

		public RabbitMqPublisher(
			IRabbitMqConnectionFactory connectionFactory,
			ILogger<RabbitMqPublisher> logger)
		{
			_connectionFactory = connectionFactory;
			_logger = logger;
			_jsonOptions = new JsonSerializerOptions
			{
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
				WriteIndented = false
			};
		}

		public async Task PublishAsync<T>(string queueName, T message, CancellationToken cancellationToken = default) where T : class
		{
			ArgumentException.ThrowIfNullOrWhiteSpace(queueName);
			ArgumentNullException.ThrowIfNull(message);

			try
			{
				var connection = await _connectionFactory.GetConnectionAsync(cancellationToken);
				await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

				// Declare queue (idempotent) - direct to queue
				await channel.QueueDeclareAsync(
					queue: queueName,
					durable: true,
					exclusive: false,
					autoDelete: false,
					arguments: null,
					cancellationToken: cancellationToken);

				var body = JsonSerializer.SerializeToUtf8Bytes(message, _jsonOptions);
				var messageId = Guid.NewGuid().ToString();

				var properties = new BasicProperties
				{
					Persistent = true,
					ContentType = "application/json",
					MessageId = messageId,
					Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds()),
					Type = typeof(T).Name
				};

				// Publish directly to queue (empty exchange, queue name as routing key)
				await channel.BasicPublishAsync(
					exchange: string.Empty,
					routingKey: queueName,
					mandatory: false,
					basicProperties: properties,
					body: body,
					cancellationToken: cancellationToken);

				_logger.LogInformation(
					"Published message {MessageType} with ID {MessageId} to queue {QueueName}",
					typeof(T).Name,
					messageId,
					queueName);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to publish message {MessageType} to queue {QueueName}", typeof(T).Name, queueName);
				throw;
			}
		}
	}
}