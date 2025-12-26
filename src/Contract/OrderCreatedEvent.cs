namespace EcaIncentoryApi.Contract
{
	public class OrderCreatedEvent
	{
		public int OrderId { get; set; }
		public required List<OrderCreatedEventItem> Items { get; set; }
	}

	public class OrderCreatedEventItem
	{
		public required string ProductSku { get; set; }
		public required int Quantity { get; set; }
	}
}