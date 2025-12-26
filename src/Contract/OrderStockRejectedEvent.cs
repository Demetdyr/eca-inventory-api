namespace EcaOrderApi.Contract
{
	public class OrderStockRejectedEvent
	{
		public int OrderId { get; set; }
		public required string Reason { get; set; }
		public required List<OrderStockRejectedEventItem> Items { get; set; }
	}

	public class OrderStockRejectedEventItem
	{
		public required string ProductSku { get; set; }
		public required int Quantity { get; set; }
	}
}