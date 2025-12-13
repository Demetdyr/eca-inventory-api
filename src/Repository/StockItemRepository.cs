using EcaInventoryApi.Data;
using EcaInventoryApi.Model;
using EcaInventoryApi.Repository.Entity;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;

namespace EcaInventoryApi.Repository
{
    public class StockItemRepository : IStockItemRepository
    {
        private readonly ApplicationDbContext _context;

        public StockItemRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<StockItemEntity>> GetAllByProductSkuAsync(List<string> skus)
        {
            return await _context.StockItems
                .FromSqlInterpolated($@"
                    SELECT * FROM stock_items
                    WHERE product_sku = ANY({skus.ToArray()})
                    FOR UPDATE
                ")
                .ToListAsync();
        }
    }
}