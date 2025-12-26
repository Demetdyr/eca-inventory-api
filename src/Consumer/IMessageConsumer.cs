namespace EcaIncentoryApi.Consumer
{
	public interface IMessageConsumer<T> where T : class
	{
		Task HandleAsync(T message, CancellationToken cancellationToken = default);
	}
}