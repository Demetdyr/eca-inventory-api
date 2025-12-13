using EcaInventoryApi.Repository.Entity;

namespace EcaInventoryApi.Repository
{
    public interface IStockItemRepository
    {
        Task<List<StockItemEntity>> GetAllByProductSkuAsync(List<string> skus);
    }
}
