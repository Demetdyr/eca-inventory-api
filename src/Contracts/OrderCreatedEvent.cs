namespace EcaInventoryApi.Contracts
{
	public class OrderCreatedEvent
	{
		public required int OrderId { get; set; }
		public required List<ReservationItem> Items { get; set; }
	}

	public class ReservationItem
	{
		public required string Sku { get; set; }
		public required int Quantity { get; set; }
	}
}