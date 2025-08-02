using Microsoft.EntityFrameworkCore;
using Order.Data.Entities;
using Order.Model;
using Order.Model.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Order.Data
{
    public class OrderRepository : IOrderRepository
    {
        private readonly OrderContext _orderContext;

        public OrderRepository(OrderContext orderContext)
        {
            _orderContext = orderContext;
        }

        public async Task<IEnumerable<OrderSummary>> GetOrdersAsync()
        {
            var orders = await _orderContext.Order
                .Include(x => x.Items)
                .Include(x => x.Status)
                .Select(x => new OrderSummary
                {
                    Id = new Guid(x.Id),
                    ResellerId = new Guid(x.ResellerId),
                    CustomerId = new Guid(x.CustomerId),
                    StatusId = new Guid(x.StatusId),
                    StatusName = x.Status.Name,
                    ItemCount = x.Items.Count,
                    TotalCost = x.Items.Sum(i => i.Quantity * i.Product.UnitCost).Value,
                    TotalPrice = x.Items.Sum(i => i.Quantity * i.Product.UnitPrice).Value,
                    CreatedDate = x.CreatedDate
                })
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();

            return orders;
        }

        public async Task<OrderDetail> GetOrderByIdAsync(Guid orderId)
        {
            var orderIdBytes = orderId.ToByteArray();

            var order = await _orderContext.Order
                .Where(x => _orderContext.Database.IsInMemory() ? x.Id.SequenceEqual(orderIdBytes) : x.Id == orderIdBytes)
                .Select(x => new OrderDetail
                {
                    Id = new Guid(x.Id),
                    ResellerId = new Guid(x.ResellerId),
                    CustomerId = new Guid(x.CustomerId),
                    StatusId = new Guid(x.StatusId),
                    StatusName = x.Status.Name,
                    CreatedDate = x.CreatedDate,
                    TotalCost = x.Items.Sum(i => i.Quantity * i.Product.UnitCost).Value,
                    TotalPrice = x.Items.Sum(i => i.Quantity * i.Product.UnitPrice).Value,
                    Items = x.Items.Select(i => new Model.OrderItem
                    {
                        Id = new Guid(i.Id),
                        OrderId = new Guid(i.OrderId),
                        ServiceId = new Guid(i.ServiceId),
                        ServiceName = i.Service.Name,
                        ProductId = new Guid(i.ProductId),
                        ProductName = i.Product.Name,
                        UnitCost = i.Product.UnitCost,
                        UnitPrice = i.Product.UnitPrice,
                        TotalCost = i.Product.UnitCost * i.Quantity.Value,
                        TotalPrice = i.Product.UnitPrice * i.Quantity.Value,
                        Quantity = i.Quantity.Value
                    })
                }).SingleOrDefaultAsync();
            var test = order.CreatedDate.Month;
            return order;
        }
        public async Task<IEnumerable<OrderDetail>> GetOrdersByStatusAsync(string statusName)
        {

            var orders = await _orderContext.Order
                .Where(x => x.Status != null && x.Status.Name.ToLower() == statusName.ToLower())
                .Select(x => new OrderDetail
                {
                    Id = new Guid(x.Id),
                    ResellerId = new Guid(x.ResellerId),
                    CustomerId = new Guid(x.CustomerId),
                    StatusId = new Guid(x.StatusId),
                    StatusName = x.Status.Name,
                    CreatedDate = x.CreatedDate,
                    TotalCost = x.Items.Sum(i => i.Quantity * i.Product.UnitCost).Value,
                    TotalPrice = x.Items.Sum(i => i.Quantity * i.Product.UnitPrice).Value,
                    Items = x.Items.Select(i => new Model.OrderItem
                    {
                        Id = new Guid(i.Id),
                        OrderId = new Guid(i.OrderId),
                        ServiceId = new Guid(i.ServiceId),
                        ServiceName = i.Service.Name,
                        ProductId = new Guid(i.ProductId),
                        ProductName = i.Product.Name,
                        UnitCost = i.Product.UnitCost,
                        UnitPrice = i.Product.UnitPrice,
                        TotalCost = i.Product.UnitCost * i.Quantity.Value,
                        TotalPrice = i.Product.UnitPrice * i.Quantity.Value,
                        Quantity = i.Quantity.Value
                    })
                }).ToListAsync();

            return orders;
        }

        public async Task<bool> UpdateOrderStatusAsync(Guid orderId, Guid statusId)
        {
            var orderIdBytes = orderId.ToByteArray();

            var order = await _orderContext.Order
                .Where(x => _orderContext.Database.IsInMemory() ? x.Id.SequenceEqual(orderIdBytes) : x.Id == orderIdBytes)
                .SingleOrDefaultAsync();
            if(order  != null)
            {
                order.StatusId = statusId.ToByteArray();
                _orderContext.Update(order);
                return await _orderContext.SaveChangesAsync() > 0;               
            }
            else
                return false;
        }

        public async Task<OrderDetail> CreateOrderAsync(PostOrderDto postOrderDto)
        {
            var newOrderId = Guid.NewGuid();

            var order = PrepareOrderObject(postOrderDto, newOrderId);
            await _orderContext.Order.AddAsync(order);
            await _orderContext.SaveChangesAsync() ;
            return await GetOrderByIdAsync(newOrderId);
        }

        private Entities.Order PrepareOrderObject(PostOrderDto postOrderDto , Guid orderId)
        {
            var newOrderId = orderId.ToByteArray();

            var order = new Entities.Order()
            {
                Id= newOrderId,
                ResellerId = postOrderDto.ResellerId.ToByteArray(),
                CustomerId = postOrderDto.CustomerId.ToByteArray(),
                CreatedDate = postOrderDto.CreatedDate,
                StatusId = postOrderDto.StatusId.ToByteArray(),
                Items = []

            };
            
            foreach (var item in postOrderDto.Items)
            {
                order.Items.Add(new Entities.OrderItem()
                {
                    Id = Guid.NewGuid().ToByteArray(),
                    ProductId = item.ProductId.ToByteArray(),
                    ServiceId = item.ServiceId.ToByteArray(),
                    Quantity = item.Quantity,
                    OrderId = newOrderId
                });
            }
            return order;
        }

        public async Task<string> CalculateProfitByMonthAsync(int month)
        {
            // I had to do this in this way, there seem to be a bug in mysql integration with
            // .net when trying to filter datetime and accessing month property
            var allOrdersItems = await _orderContext.Order.Select(x => new 
            { Month = Convert.ToInt32(x.CreatedDate.ToString("MM")), Items = x.Items }).ToListAsync();
            decimal? profitDone = 0;
            foreach (var singleOrderItems in allOrdersItems.Where(x=>x.Month == month).Select(x=>x.Items))
            {
                foreach (var item in singleOrderItems) 
                {
                    profitDone += item.Quantity * (item.Product.UnitPrice - item.Product.UnitCost);
                }
            }
            return $"Profit made in month number {month} through all years is {profitDone}";

        }

        public async Task<string> CalculateProfitByMonthAndYearAsync(int month, int year)
        {
            // I had to do this in this way, there seem to be a bug in mysql integration with
            // .net when trying to filter datetime and accessing month property
            var allOrdersItems = await _orderContext.Order.Select(x => new
            { Month = Convert.ToInt32(x.CreatedDate.ToString("MM")), Year = Convert.ToInt32(x.CreatedDate.ToString("yyyy")),
                Items = x.Items }).ToListAsync();
            decimal? profitDone = 0;
            foreach (var singleOrderItems in allOrdersItems.Where(x => x.Month == month && x.Year == year).Select(x => x.Items))
            {
                foreach (var item in singleOrderItems)
                {
                    profitDone += item.Quantity * (item.Product.UnitPrice - item.Product.UnitCost);
                }
            }
            return $"Profit made in month number {month} in year {year} is {profitDone}";
        }
    }
}
