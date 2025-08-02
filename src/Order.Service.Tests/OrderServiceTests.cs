﻿using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using NUnit.Framework;
using Order.Data;
using Order.Data.Entities;
using Order.Model.Dtos;
using System;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Order.Service.Tests
{
    public class OrderServiceTests
    {
        private IOrderService _orderService;
        private IOrderRepository _orderRepository;
        private IStatusRepository _statusRepository;
        private IProductRepository _productRepository;
        private IServiceRepository _serviceRepository;

        private OrderContext _orderContext;
        private DbConnection _connection;
        private readonly Guid _orderStatusCompletedGuid = Guid.NewGuid();
        private readonly byte[] _orderStatusCreatedId = Guid.NewGuid().ToByteArray();
        private readonly Guid _orderServiceEmailIdGuid = Guid.NewGuid();
        private readonly Guid _orderProductEmailIdGuid = Guid.NewGuid();


        [SetUp]
        public async Task Setup()
        {
            var options = new DbContextOptionsBuilder<OrderContext>()
                .UseSqlite(CreateInMemoryDatabase())
                .EnableDetailedErrors(true)
                .EnableSensitiveDataLogging(true)
                .Options;

            _connection = RelationalOptionsExtension.Extract(options).Connection;

            _orderContext = new OrderContext(options);
            _orderContext.Database.EnsureDeleted();
            _orderContext.Database.EnsureCreated();

            _orderRepository = new OrderRepository(_orderContext);
            _statusRepository = new StatusRepository(_orderContext);
            _productRepository = new ProductRepository(_orderContext);
            _serviceRepository = new ServiceRepository(_orderContext);
            _orderService = new OrderService(_orderRepository, _statusRepository, _serviceRepository , _productRepository);

            await AddReferenceDataAsync(_orderContext);
        }

        [TearDown]
        public void TearDown()
        {
            _connection.Dispose();
            _orderContext.Dispose();
        }


        private static DbConnection CreateInMemoryDatabase()
        {
            var connection = new SqliteConnection("Filename=:memory:");
            connection.Open();

            return connection;
        }

        [Test]
        public async Task GetOrdersAsync_ReturnsCorrectNumberOfOrders()
        {
            // Arrange
            var orderId1 = Guid.NewGuid();
            await AddOrder(orderId1, 1);

            var orderId2 = Guid.NewGuid();
            await AddOrder(orderId2, 2);

            var orderId3 = Guid.NewGuid();
            await AddOrder(orderId3, 3);

            // Act
            var orders = await _orderService.GetOrdersAsync();

            // Assert
            Assert.AreEqual(3, orders.Count());
        }

        [Test]
        public async Task GetOrdersAsync_ReturnsOrdersWithCorrectTotals()
        {
            // Arrange
            var orderId1 = Guid.NewGuid();
            await AddOrder(orderId1, 1);

            var orderId2 = Guid.NewGuid();
            await AddOrder(orderId2, 2);

            var orderId3 = Guid.NewGuid();
            await AddOrder(orderId3, 3);

            // Act
            var orders = await _orderService.GetOrdersAsync();

            // Assert
            var order1 = orders.SingleOrDefault(x => x.Id == orderId1);
            var order2 = orders.SingleOrDefault(x => x.Id == orderId2);
            var order3 = orders.SingleOrDefault(x => x.Id == orderId3);

            Assert.AreEqual(0.8m, order1.TotalCost);
            Assert.AreEqual(0.9m, order1.TotalPrice);

            Assert.AreEqual(1.6m, order2.TotalCost);
            Assert.AreEqual(1.8m, order2.TotalPrice);

            Assert.AreEqual(2.4m, order3.TotalCost);
            Assert.AreEqual(2.7m, order3.TotalPrice);
        }

        [Test]
        public async Task GetOrderByIdAsync_ReturnsCorrectOrder()
        {
            // Arrange
            var orderId1 = Guid.NewGuid();
            await AddOrder(orderId1, 1);

            // Act
            var order = await _orderService.GetOrderByIdAsync(orderId1);

            // Assert
            Assert.AreEqual(orderId1, order.Id);
        }

        [Test]
        public async Task GetOrderByIdAsync_ReturnsCorrectOrderItemCount()
        {
            // Arrange
            var orderId1 = Guid.NewGuid();
            await AddOrder(orderId1, 1);

            // Act
            var order = await _orderService.GetOrderByIdAsync(orderId1);

            // Assert
            Assert.AreEqual(1, order.Items.Count());
        }

        [Test]
        public async Task GetOrderByIdAsync_ReturnsOrderWithCorrectTotals()
        {
            // Arrange
            var orderId1 = Guid.NewGuid();
            await AddOrder(orderId1, 2);

            // Act
            var order = await _orderService.GetOrderByIdAsync(orderId1);

            // Assert
            Assert.AreEqual(1.6m, order.TotalCost);
            Assert.AreEqual(1.8m, order.TotalPrice);
        }

        private async Task AddOrder(Guid orderId, int quantity)
        {
            var orderIdBytes = orderId.ToByteArray();
            _orderContext.Order.Add(new Data.Entities.Order
            {
                Id = orderIdBytes,
                ResellerId = Guid.NewGuid().ToByteArray(),
                CustomerId = Guid.NewGuid().ToByteArray(),
                CreatedDate = DateTime.Now,
                StatusId = _orderStatusCreatedId,
            });

            _orderContext.OrderItem.Add(new OrderItem
            {
                Id = Guid.NewGuid().ToByteArray(),
                OrderId = orderIdBytes,
                ServiceId = _orderServiceEmailIdGuid.ToByteArray(),
                ProductId = _orderProductEmailIdGuid.ToByteArray(),
                Quantity = quantity
            });

            await _orderContext.SaveChangesAsync();
        }
        [Test]
        public async Task GetOrderByStatuAsync_ReturnsCorrectOrder()
        {
            // Arrange
            var orderId1 = Guid.NewGuid();
            await AddOrder(orderId1, 1);

            // Act
            var orders= await _orderService.GetOrdersByStatusAsync("created");

            // Assert
            Assert.AreEqual("Created", orders.First().StatusName);
        }

        [Test]
        public async Task UpdateOrderStatusAsync_ReturnsTrueAndDataUpdatedSuccessfully()
        {
            // Arrange
            var orderId1 = Guid.NewGuid();
            await AddOrder(orderId1, 1);

            // Act
            var result = await _orderService.UpdateOrderStatusAsync(orderId1, _orderStatusCompletedGuid);

            // Assert
            Assert.AreEqual(true, result);
            Assert.AreEqual("Completed", _orderContext.Order.First().Status.Name);

        }
        [Test]
        public async Task CreateOrderAsync_ReturnsNewlyCreatedOrderDetails()
        {
            // Arrange
            var postOrderDto = new PostOrderDto()
            {
                CreatedDate = DateTime.Now,
                CustomerId = Guid.NewGuid(),
                ResellerId = Guid.NewGuid(),
                StatusId = _orderStatusCompletedGuid,
                Items = new System.Collections.Generic.List<OrderItemDto>()
                {
                    new OrderItemDto()
                    {
                        ProductId =  _orderProductEmailIdGuid,
                        Quantity = 4,
                        ServiceId =  _orderServiceEmailIdGuid
                    }
                }
            };

            // Act
            var result = await _orderService.CreateOrderAsync(postOrderDto);

            // Assert
            Assert.NotNull( result);
            Assert.AreEqual(1, _orderContext.Order.Count());

        }
        [Test]
        public async Task CalculateProfitByMonthAsync_ReturnsStringWithCorrectData ()
        {
            // Arrange
            var orderId1 = Guid.NewGuid();
            await AddOrder(orderId1, 5);

            // Act
            var result = await _orderService.CalculateProfitByMonthAsync(8);

            // Assert
            Assert.AreEqual($"Profit made in month number 8 through all years is 0.5",
                result);

        }
        [Test]
        public async Task CalculateProfitByMonthAndYearAsync_ReturnsStringWithCorrectData()
        {
            // Arrange
            var orderId1 = Guid.NewGuid();
            await AddOrder(orderId1, 7);

            // Act
            var result = await _orderService.CalculateProfitByMonthAndYearAsync(8,2025);

            // Assert
            Assert.AreEqual($"Profit made in month number 8 in year 2025 is 0.7",
                result);

        }
        private async Task AddReferenceDataAsync(OrderContext orderContext)
        {
            orderContext.OrderStatus.Add(new OrderStatus
            {
                Id = _orderStatusCreatedId,
                Name = "Created",
            }); 
            
            orderContext.OrderStatus.Add(new OrderStatus
            {
                Id = _orderStatusCompletedGuid.ToByteArray(),
                Name = "Completed",
            });

            orderContext.OrderService.Add(new Data.Entities.OrderService
            {
                Id = _orderServiceEmailIdGuid.ToByteArray(),
                Name = "Email"
            });

            orderContext.OrderProduct.Add(new OrderProduct
            {
                Id = _orderProductEmailIdGuid.ToByteArray(),
                Name = "100GB Mailbox",
                UnitCost = 0.8m,
                UnitPrice = 0.9m,
                ServiceId = _orderServiceEmailIdGuid.ToByteArray(),
            });

            await orderContext.SaveChangesAsync();
        }
    }
}
