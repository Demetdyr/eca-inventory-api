using Microsoft.EntityFrameworkCore;
using EcaInventoryApi.Repository.Entity;
using System.Text.RegularExpressions;

namespace EcaInventoryApi.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<StockItemEntity> StockItems { get; set; }
        public DbSet<ReservationEntity> Reservations { get; set; }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<ReservationEntity>()
				.Property(o => o.CreatedAt)
				.ValueGeneratedOnAdd();
			modelBuilder.Entity<StockItemEntity>()
				.Property(o => o.UpdatedAt)
				.ValueGeneratedOnAddOrUpdate();
			

			foreach (var entity in modelBuilder.Model.GetEntityTypes())
			{
				var name = entity.GetTableName();
				if (name != null)
				{
					entity.SetTableName(ToSnakeCase(name));
				}

				foreach (var property in entity.GetProperties())
				{
					property.SetColumnName(ToSnakeCase(property.Name));
				}

				foreach (var key in entity.GetKeys())
				{
					var keyName = key.GetName();
					if (keyName != null) key.SetName(ToSnakeCase(keyName));
				}

				foreach (var fk in entity.GetForeignKeys())
				{
					var getConstraintName = fk.GetConstraintName();
					if (getConstraintName != null) fk.SetConstraintName(ToSnakeCase(getConstraintName));
				}

				foreach (var index in entity.GetIndexes())
				{
					var getDatabaseName = index.GetDatabaseName();
					if (getDatabaseName != null) index.SetDatabaseName(ToSnakeCase(getDatabaseName));
				}
			}
		}

        private static string ToSnakeCase(string name)
        {
            if (string.IsNullOrEmpty(name))
                return name;

            var startUnderscores = Regex.Match(name, @"^_+");
            return startUnderscores + Regex.Replace(name, @"([a-z0-9])([A-Z])", "$1_$2").ToLower();
        }
    }

}