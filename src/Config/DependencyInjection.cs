using EcaIncentoryApi.Consumer;
using EcaOrderApi.Messaging;

namespace EcaOrderApi.Config
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddRabbitMq(this IServiceCollection services, IConfiguration configuration)
		{
			services.Configure<RabbitMqSettings>(configuration.GetSection(RabbitMqSettings.SectionName));

			services.AddSingleton<IRabbitMqConnectionFactory, RabbitMqConnectionFactory>();
			services.AddSingleton<IMessagePublisher, RabbitMqPublisher>();

			return services;
		}

		public static IServiceCollection AddRabbitMqConsumer<TMessage, THandler>(this IServiceCollection services, string queueName)
			where TMessage : class
			where THandler : class, IMessageConsumer<TMessage>
		{
			services.AddScoped<THandler>();
			
			services.AddHostedService(sp =>
			{
				var connectionFactory = sp.GetRequiredService<IRabbitMqConnectionFactory>();
				var scopeFactory = sp.GetRequiredService<IServiceScopeFactory>();
				var logger = sp.GetRequiredService<ILogger<RabbitMqConsumerService<TMessage, THandler>>>();

				return new RabbitMqConsumerService<TMessage, THandler>(
					connectionFactory,
					scopeFactory,
					logger,
					queueName);
			});

			return services;
		}
	}
}