using EcaInventoryApi.Contract;
using EcaInventoryApi.Service;
using EcaInventoryApi.Data;
using EcaInventoryApi.Model;
using EcaInventoryApi.Repository;
using EcaInventoryApi.Repository.Entity;
using EcaOrderApi.Contract;
using EcaOrderApi.Messaging;
using Microsoft.EntityFrameworkCore;

namespace EcaInventoryApi.Service
{
    public class ReservationService : IReservationService
    {
		private readonly IStockItemRepository _stockItemRepository;
		private readonly ApplicationDbContext _context;
		private readonly IReservationRepository _reservationRepository;
		private readonly ILogger<ReservationService> _logger;
		private readonly IMessagePublisher _messagePublisher;

		public ReservationService(IStockItemRepository stockItemRepository, ApplicationDbContext context, IReservationRepository reservationRepository, ILogger<ReservationService> logger, IMessagePublisher messagePublisher)
		{
			_stockItemRepository = stockItemRepository;
			_context = context;
			_reservationRepository = reservationRepository;
			_logger = logger;
			_messagePublisher = messagePublisher;
		}

		public async Task CreateReservation(ReserveStockCommand message)
		{
			_logger.LogInformation("Creating reservation for OrderId: {OrderId}", message.OrderId);

			await using var transaction = await _context.Database.BeginTransactionAsync();

			try
			{
				// Get product SKUs from the order
				var productSkus = message.Items.Select(i => i.ProductSku).ToList();
				
				// Get stock items for these products
				var stockItems = await _stockItemRepository.GetAllByProductSkuAsync(productSkus);

				// Check if all items have sufficient stock
				var insufficientStockItems = new List<string>();
				foreach (var item in message.Items)
				{
					var stockItem = stockItems.FirstOrDefault(s => s.ProductSku == item.ProductSku);
					var availableQuantity = stockItem != null 
						? stockItem.Quantity - stockItem.ReservedQuantity 
						: 0;

					if (stockItem == null || availableQuantity < item.Quantity)
					{
						insufficientStockItems.Add(item.ProductSku);
					}
				}

				// If any item has insufficient stock, reject the order
				if (insufficientStockItems.Count > 0)
				{
					_logger.LogWarning("Insufficient stock for OrderId: {OrderId}, Products: {Products}", 
						message.OrderId, string.Join(", ", insufficientStockItems));

					var rejectedEvent = new OrderStockRejectedEvent
					{
						OrderId = message.OrderId,
						Reason = $"Insufficient stock for products: {string.Join(", ", insufficientStockItems)}",
						Items = message.Items.Select(i => new OrderStockRejectedEventItem
						{
							ProductSku = i.ProductSku,
							Quantity = i.Quantity
						}).ToList()
					};
					await _messagePublisher.PublishAsync("inventory.order.stock.rejected", rejectedEvent);
					return;
				}

				// Create reservations and update stock
				var reservations = new List<ReservationEntity>();
				foreach (var item in message.Items)
				{
					var stockItem = stockItems.First(s => s.ProductSku == item.ProductSku);
					
					// Reserve stock
					stockItem.ReservedQuantity += item.Quantity;
					stockItem.UpdatedAt = DateTime.UtcNow;

					// Create reservation record
					var reservation = new ReservationEntity
					{
						OrderId = message.OrderId,
						ProductSku = item.ProductSku,
						Quantity = item.Quantity,
						Status = ReservationStatus.Pending,
						ExpiresAt = DateTime.UtcNow.AddMinutes(30),
						CreatedAt = DateTime.UtcNow
					};
					reservations.Add(reservation);
				}

				await _reservationRepository.AddReservation(reservations);
				await _context.SaveChangesAsync();
				await transaction.CommitAsync();

				_logger.LogInformation("Successfully created reservations for OrderId: {OrderId}", message.OrderId);

				// Send stock reserved event
				var reservedEvent = new OrderStockReservedEvent
				{
					OrderId = message.OrderId,
					Items = message.Items.Select(i => new OrderStockReservedEventItem
					{
						ProductSku = i.ProductSku,
						Quantity = i.Quantity
					}).ToList()
				};
				await _messagePublisher.PublishAsync("inventory.order.stock.reserved", reservedEvent);
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				_logger.LogError(ex, "Failed to create reservation for OrderId: {OrderId}", message.OrderId);
				throw;
			}
		}

		public async Task ConfirmReservation(int orderId, CancellationToken cancellationToken = default)
		{
			_logger.LogInformation("Confirming reservation for OrderId: {OrderId}", orderId);

			await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

			try
			{
				// Get all pending reservations for this order
				var reservations = await _reservationRepository.GetByOrderIdAsync(orderId, cancellationToken);

				if (reservations.Count == 0)
				{
					_logger.LogWarning("No reservations found for OrderId: {OrderId}", orderId);
					return;
				}

				var pendingReservations = reservations
					.Where(r => r.Status == ReservationStatus.Pending)
					.ToList();

				if (pendingReservations.Count == 0)
				{
					_logger.LogWarning("No pending reservations found for OrderId: {OrderId}", orderId);
					return;
				}

				// Get product SKUs from reservations
				var productSkus = pendingReservations.Select(r => r.ProductSku).ToList();

				// Get stock items with lock
				var stockItems = await _stockItemRepository.GetAllByProductSkuAsync(productSkus);

				foreach (var reservation in pendingReservations)
				{
					var stockItem = stockItems.FirstOrDefault(s => s.ProductSku == reservation.ProductSku);

					if (stockItem == null)
					{
						_logger.LogError("Stock item not found for ProductSku: {ProductSku}", reservation.ProductSku);
						throw new InvalidOperationException($"Stock item not found for ProductSku: {reservation.ProductSku}");
					}

					// Decrease actual stock quantity
					stockItem.Quantity -= reservation.Quantity;
					// Clear reserved quantity
					stockItem.ReservedQuantity -= reservation.Quantity;
					stockItem.UpdatedAt = DateTime.UtcNow;

					// Update reservation status to confirmed
					reservation.Status = ReservationStatus.Confirmed;

					_logger.LogInformation(
						"Confirmed reservation for ProductSku: {ProductSku}, Quantity: {Quantity}, New Stock: {NewStock}",
						reservation.ProductSku,
						reservation.Quantity,
						stockItem.Quantity);
				}

				await _context.SaveChangesAsync(cancellationToken);
				await transaction.CommitAsync(cancellationToken);

				_logger.LogInformation("Successfully confirmed all reservations for OrderId: {OrderId}", orderId);
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync(cancellationToken);
				_logger.LogError(ex, "Failed to confirm reservation for OrderId: {OrderId}", orderId);
				throw;
			}
		}
    }
}