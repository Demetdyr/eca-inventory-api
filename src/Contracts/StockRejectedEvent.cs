namespace EcaInventoryApi.Contracts
{
	public class StockRejectedEvent
    {
        public required int OrderId { get; set; }
        public required List<ReservationItem> Items { get; set; }
        public string? Reason { get; set; }
    }
}