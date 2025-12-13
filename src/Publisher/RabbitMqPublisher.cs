using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace EcaInventoryApi.Publisher
{
	public class RabbitMqPublisher : IRabbitMqPublisher
	{
        private readonly IConnection _connection;
        private readonly string _exchangeName;

		private readonly ConcurrentDictionary<ulong, TaskCompletionSource<bool>> _pendingConfirms 
            = new();

		public RabbitMqPublisher(IConnection connection, string exchangeName = "")
		{
			_connection = connection ?? throw new ArgumentNullException(nameof(connection));
			_exchangeName = exchangeName;
		}

		public async Task PublishAsync(object message, string routingKey = "")
		{
            IChannel channel = await _connection.CreateChannelAsync();

			await channel.QueueDeclareAsync(
				queue: routingKey,
				durable: false,
				exclusive: false,
				autoDelete: false,
				arguments: null
			);

			if (!string.IsNullOrEmpty(_exchangeName))
			{
				await channel.ExchangeDeclareAsync(
					exchange: _exchangeName,
					type: ExchangeType.Direct,
					durable: true,
					autoDelete: false,
					arguments: null,
					noWait: false,
					cancellationToken: CancellationToken.None
				);
			}
			
			channel.BasicAcksAsync += (sender, ea) =>
            {
                if (_pendingConfirms.TryRemove(ea.DeliveryTag, out var tcs))
                {
                    tcs.TrySetResult(true);
                }
                return Task.CompletedTask;
            };

            channel.BasicNacksAsync += (sender, ea) =>
            {
                if (_pendingConfirms.TryRemove(ea.DeliveryTag, out var tcs))
                {
                    tcs.TrySetException(new Exception($"broker nack for seq {ea.DeliveryTag}"));
                }
                return Task.CompletedTask;
            };

			string json = JsonSerializer.Serialize(message);
			var body = Encoding.UTF8.GetBytes(json);

			var props = new BasicProperties();
            props.Persistent = true;
			props.ContentType = "application/json";
			
			ulong seqNo = await channel.GetNextPublishSequenceNumberAsync();
            var tcsConfirm = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            _pendingConfirms[seqNo] = tcsConfirm;

			await channel.BasicPublishAsync(
                exchange: _exchangeName,
                routingKey: routingKey,
                mandatory: false,
                basicProperties: props,
                body: body,
                cancellationToken: CancellationToken.None
			);

			await tcsConfirm.Task.ConfigureAwait(false);

            await channel.CloseAsync();
            await channel.DisposeAsync();
		}
	}
}