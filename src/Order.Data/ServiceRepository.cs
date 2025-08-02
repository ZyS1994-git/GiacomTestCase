using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Order.Data
{
    public class ServiceRepository : IServiceRepository
    {
        private readonly OrderContext _orderContext;

        public ServiceRepository(OrderContext orderContext)
        {
            _orderContext = orderContext;
        }

        public async Task<bool> ExistsAsync(Guid serviceId)
        {
            var serviceIdBytes = serviceId.ToByteArray();

            return await _orderContext.OrderService.AnyAsync(x => x.Id == serviceIdBytes);
        }
    }
}
