namespace EcaOrderApi.Contract
{
	public class OrderStockReservedEvent
	{
		public int OrderId { get; set; }
		public required List<OrderStockReservedEventItem> Items { get; set; }
	}

	public class OrderStockReservedEventItem
	{
		public required string ProductSku { get; set; }
		public required int Quantity { get; set; }
	}
}