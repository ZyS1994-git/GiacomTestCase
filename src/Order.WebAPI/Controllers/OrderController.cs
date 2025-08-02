using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Order.Data.Entities;
using Order.Model.Dtos;
using Order.Service;
using Order.Service.Exceptions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OrderService.WebAPI.Controllers
{
    [ApiController]
    [Route("orders")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Get()
        {
            var orders = await _orderService.GetOrdersAsync();
            return Ok(orders);
        }

        [HttpGet("{orderId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetOrderById(Guid orderId)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);
            if (order != null)
            {
                return Ok(order);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpGet("GetOrdersByStatus/{statusName}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Route("GetOrdersByStatus")]
        public async Task<IActionResult> GetOrdersByStatus([FromQuery]string statusName)
        {
            var orders = await _orderService.GetOrdersByStatusAsync(statusName);
            if (orders.Any())
            {
                return Ok(orders);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPut("{orderId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Put(Guid orderId , [FromBody] UpdateOrderStatusDto updateOrderStatus)
        {
            try
            {
                var updated = await _orderService.UpdateOrderStatusAsync(orderId, updateOrderStatus.StatusId);
                if (updated)
                {
                    return NoContent();
                }
                else
                {
                    return NotFound();
                }
            }
            catch (BadRequestException ex) 
            { 
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Post([FromBody] PostOrderDto postOrderDto)
        {

            try
            {
                var insertedOdrer = await _orderService.CreateOrderAsync(postOrderDto);
                if (insertedOdrer != null)
                {
                    return Created($"/api/orders/{insertedOdrer.Id}",insertedOdrer);
                }
                else
                {
                    return BadRequest("Bad input, order was not created");

                }
            }
            catch (BadRequestException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("CalculateProfitByMonth/{monthNum}")]
        [Route("CalculateProfitByMonth")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> CalculateProfitByMonth(int monthNum)
        {
            var message = await _orderService.CalculateProfitByMonthAsync(monthNum);
            return Ok(message);
        }

        [HttpGet("CalculateProfitByMonthAndYear/{monthNum}/{yearNum}")]
        [Route("CalculateProfitByMonthAndYear")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> CalculateProfitByMonthAndYear(int monthNum, int yearNum)
        {
            var message = await _orderService.CalculateProfitByMonthAndYearAsync(monthNum, yearNum);
            return Ok(message);
        }
    }
}
