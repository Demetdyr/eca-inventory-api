namespace EcaInventoryApi.Contract
{
	public class ReserveStockCommand
	{
		public int OrderId { get; set; }
		public required List<ReserveStockCommandItem> Items { get; set; }
	}

	public class ReserveStockCommandItem
	{
		public required string ProductSku { get; set; }
		public required int Quantity { get; set; }
	}
}