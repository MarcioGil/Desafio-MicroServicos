using Xunit;
using SalesService.Services;
using SalesService.Data;
using SalesService.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;

namespace MicroservicesEcommerce.Tests
{
    public class SalesServiceTests
    {
        private SalesContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<SalesContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Unique DB for each test
                .Options;
            var context = new SalesContext(options);
            context.Database.EnsureCreated();
            return context;
        }

        [Fact]
        public async Task CreateOrderAsync_ShouldCreateOrderWithPendingStatus()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var service = new SalesManagementService(context);
            var orderDto = new OrderCreationDto
            {
                CustomerName = "Test Customer",
                Items = new List<OrderItemDto>
                {
                    new OrderItemDto { ProductId = 1, Quantity = 2 },
                    new OrderItemDto { ProductId = 2, Quantity = 1 }
                }
            };

            // Act
            var createdOrder = await service.CreateOrderAsync(orderDto);

            // Assert
            Assert.NotNull(createdOrder);
            Assert.Equal("Test Customer", createdOrder.CustomerName);
            Assert.Equal("Pending", createdOrder.Status);
            Assert.Equal(2, createdOrder.Items.Count);
            var orderInDb = await context.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == createdOrder.Id);
            Assert.NotNull(orderInDb);
        }

        [Fact]
        public async Task UpdateOrderStatusAsync_ShouldUpdateStatus()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var service = new SalesManagementService(context);
            var orderDto = new OrderCreationDto
            {
                CustomerName = "Test Customer",
                Items = new List<OrderItemDto>
                {
                    new OrderItemDto { ProductId = 1, Quantity = 1 }
                }
            };
            var order = await service.CreateOrderAsync(orderDto);
            int orderId = order.Id;
            string newStatus = "Confirmed";

            // Act
            var success = await service.UpdateOrderStatusAsync(orderId, newStatus);

            // Assert
            Assert.True(success);
            var updatedOrder = await service.GetOrderByIdAsync(orderId);
            Assert.Equal(newStatus, updatedOrder.Status);
        }
    }
}
