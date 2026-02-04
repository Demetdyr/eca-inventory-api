using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EcaInventoryApi.Model;

namespace EcaInventoryApi.Repository.Entity
{
    [Table("stock_items")]
    public class StockItemEntity
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public required string ProductSku { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        public int ReservedQuantity { get; set; }

        [Required]
        public DateTime UpdatedAt { get; set; }

        public StockItem ToModel()
		{
			return new StockItem
			{
				Id = Id,
				ProductSku = ProductSku,
				Quantity = Quantity,
				ReservedQuantity = ReservedQuantity,
				UpdatedAt = UpdatedAt
			};
		}

        public static StockItemEntity FromModel(StockItem stockItem)
		{
			return new StockItemEntity
			{
				Id = stockItem.Id,
				ProductSku = stockItem.ProductSku,
				Quantity = stockItem.Quantity,
				ReservedQuantity = stockItem.ReservedQuantity,
				UpdatedAt = stockItem.UpdatedAt
			};
		}

    }
}