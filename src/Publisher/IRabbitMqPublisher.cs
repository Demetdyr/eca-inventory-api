namespace EcaInventoryApi.Publisher
{
	public interface IRabbitMqPublisher
    {
        Task PublishAsync(object message, string routingKey);
    }
}