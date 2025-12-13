namespace EcaInventoryApi.Common
{
	public class NotFoundException : Exception
	{
		public string? Key { get; set; }
		public object? ResourceId { get; set; }

		public NotFoundException(string key, object? resourceId) : base($"{key} not found: {resourceId}")
		{
			ResourceId = resourceId;
			Key = key;
		}
	}
}