using EcaInventoryApi.Config;
using RabbitMQ.Client;

namespace EcaInventoryApi.Config
{
    public static class RabbitMqConnectionFactory
    {
        public static async Task<IConnection> CreateConnectionAsync(RabbitMqOptions opts)
        {
            var factory = new ConnectionFactory
			{
				HostName = opts.HostName,
				UserName = opts.UserName,
				Password = opts.Password,
				VirtualHost = opts.VirtualHost,
				Port = opts.Port
			};

            return await factory.CreateConnectionAsync();
        }
    }
}