namespace EcaInventoryApi.Model
{
    public class StockItem
    {
        public int Id { get; set; }
        public string ProductSku { get; set; }
        public int Quantity { get; set; }
        public int ReservedQuantity { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}