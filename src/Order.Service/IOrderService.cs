using Order.Model;
using Order.Model.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Order.Service
{
    public interface IOrderService
    {
        Task<IEnumerable<OrderSummary>> GetOrdersAsync();
        
        Task<OrderDetail> GetOrderByIdAsync(Guid orderId);
        Task<IEnumerable<OrderDetail>> GetOrdersByStatusAsync(string statusName);
        Task<bool> UpdateOrderStatusAsync(Guid orderId, Guid statusId);
        Task<OrderDetail> CreateOrderAsync(PostOrderDto postOrderDto);
        Task<string> CalculateProfitByMonthAsync(int month);
        Task<string> CalculateProfitByMonthAndYearAsync(int month, int year);



    }
}
