using System.ComponentModel.DataAnnotations;

namespace EcaInventoryApi.Controller.Dto
{
    public class CreateStockItemRequest
    {
        [Required]
        public required string ProductSku { get; set; } 

        [Required]
        public int Quantity { get; set; }
    }
}