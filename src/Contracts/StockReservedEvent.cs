namespace EcaInventoryApi.Contracts
{
	public class StockReservedEvent
    {
        public required int OrderId { get; set; }
        public required List<ReservationItem> Items { get; set; }
    }

}