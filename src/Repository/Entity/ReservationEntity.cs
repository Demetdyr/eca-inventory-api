using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EcaInventoryApi.Model;

namespace EcaInventoryApi.Repository.Entity
{
    [Table("reservations")]
    public class ReservationEntity
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int OrderId { get; set; }
        [Required]
        public required string ProductSku { get; set; }
        [Required]
        public int Quantity { get; set; }
        [Required]
        public ReservationStatus Status { get; set; }
        [Required] 
        public DateTime ExpiresAt { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; }

        public Reservation ToModel()
        {
            return new Reservation
            {
                Id = Id,
                OrderId = OrderId,
                ProductSku = ProductSku,
                Quantity = Quantity,
                Status = Status,
                ExpiresAt = ExpiresAt,
                CreatedAt = CreatedAt
            };
        }
        public static ReservationEntity FromModel(Reservation reservation)
        {
            return new ReservationEntity
            {
                Id = reservation.Id,
                OrderId = reservation.OrderId,
                ProductSku = reservation.ProductSku,
                Quantity = reservation.Quantity,
                Status = reservation.Status,
                ExpiresAt = reservation.ExpiresAt,
                CreatedAt = reservation.CreatedAt
            };
        }
    }
}
