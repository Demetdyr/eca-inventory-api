namespace EcaInventoryApi.Model
{
    public class Reservation
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public required string ProductSku { get; set; }
        public int Quantity { get; set; }
        public ReservationStatus Status { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}