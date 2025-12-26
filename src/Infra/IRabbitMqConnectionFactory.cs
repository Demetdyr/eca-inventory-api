using RabbitMQ.Client;

namespace EcaOrderApi.Messaging
{
	public interface IRabbitMqConnectionFactory
	{
		Task<IConnection> GetConnectionAsync(CancellationToken cancellationToken = default);
	}
}
