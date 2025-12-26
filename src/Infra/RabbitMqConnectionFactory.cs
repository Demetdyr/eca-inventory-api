
using EcaOrderApi.Config;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace EcaOrderApi.Messaging
{
	public class RabbitMqConnectionFactory : IRabbitMqConnectionFactory, IAsyncDisposable
	{
		private readonly RabbitMqSettings _settings;
		private readonly SemaphoreSlim _semaphore = new(1, 1);
		private IConnection? _connection;
		private bool _disposed;

		public RabbitMqConnectionFactory(IOptions<RabbitMqSettings> settings)
		{
			_settings = settings.Value;
		}

		public async Task<IConnection> GetConnectionAsync(CancellationToken cancellationToken = default)
		{
			if (_connection is { IsOpen: true })
				return _connection;

			await _semaphore.WaitAsync(cancellationToken);
			try
			{
				if (_connection is { IsOpen: true })
					return _connection;

				var factory = new ConnectionFactory
				{
					HostName = _settings.HostName,
					Port = _settings.Port,
					UserName = _settings.UserName,
					Password = _settings.Password,
					VirtualHost = _settings.VirtualHost,
					AutomaticRecoveryEnabled = true,
					NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
				};

				_connection = await factory.CreateConnectionAsync(cancellationToken);
				return _connection;
			}
			finally
			{
				_semaphore.Release();
			}
		}

		public async ValueTask DisposeAsync()
		{
			if (_disposed) return;

			_disposed = true;

			if (_connection is not null)
			{
				await _connection.CloseAsync();
				_connection.Dispose();
			}

			_semaphore.Dispose();
			GC.SuppressFinalize(this);
		}
	}
}