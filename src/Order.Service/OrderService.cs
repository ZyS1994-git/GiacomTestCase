using Order.Data;
using Order.Model;
using Order.Model.Dtos;
using Order.Service.Exceptions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Order.Service
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IStatusRepository _statusRepository;
        private readonly IServiceRepository _serviceRepository;
        private readonly IProductRepository _productRepository;
        public OrderService(IOrderRepository orderRepository, IStatusRepository statusRepository,
            IServiceRepository serviceRepository, IProductRepository productRepository)
        {
            _orderRepository = orderRepository;
            _statusRepository = statusRepository;
            _serviceRepository = serviceRepository;
            _productRepository = productRepository;
        }

        public async Task<IEnumerable<OrderSummary>> GetOrdersAsync()
        {
            var orders = await _orderRepository.GetOrdersAsync();
            return orders;
        }

        public async Task<OrderDetail> GetOrderByIdAsync(Guid orderId)
        {
            var order = await _orderRepository.GetOrderByIdAsync(orderId);
            return order;
        }
        public async Task<IEnumerable<OrderDetail>> GetOrdersByStatusAsync(string statusName)
        {
            var orders = await _orderRepository.GetOrdersByStatusAsync(statusName);
            return orders;
        }
        public async Task<bool> UpdateOrderStatusAsync(Guid orderId, Guid statusId)
        {
            var statusExist =  await _statusRepository.ExistsAsync(statusId);
            if (!statusExist)
                throw new BadRequestException("Bad status sent");
            return await _orderRepository.UpdateOrderStatusAsync(orderId, statusId);
            
        }

        public async Task<OrderDetail> CreateOrderAsync(PostOrderDto postOrderDto)
        {
            await ValidateData(postOrderDto);
            return await _orderRepository.CreateOrderAsync(postOrderDto);
        }

        private async Task<bool> ValidateData(PostOrderDto postOrderDto)
        {
            //Note: customer id and reseller id needs to be validated alson but customer table does not exist
            var statusExist = await _statusRepository.ExistsAsync(postOrderDto.StatusId);
            if (!statusExist)
                throw new BadRequestException("Bad status sent");

            foreach(var item in postOrderDto.Items)
            {
                var serviceExist = await _serviceRepository.ExistsAsync(item.ServiceId);
                if(!serviceExist)
                    throw new BadRequestException("Bad service sent");
                var productExists = await _productRepository.ExistsAsync(item.ProductId, item.ServiceId);
                if (!productExists)
                    throw new BadRequestException("Bad product sent or product does not belong to the linked service");
            }

            return true;
        }

        public async Task<string> CalculateProfitByMonthAsync(int month)
        {
            if (month < 1 || month > 12)
                throw new BadRequestException("Please insert valid month number");

            return await _orderRepository.CalculateProfitByMonthAsync(month);
        }

        public async Task<string> CalculateProfitByMonthAndYearAsync(int month , int year)
        {
            if (month < 1 || month > 12)
                throw new BadRequestException("Please insert valid month number");

            return await _orderRepository.CalculateProfitByMonthAndYearAsync(month, year);
        }
    }
}
