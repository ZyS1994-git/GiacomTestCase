using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Data
{
    public class ProductRepository : IProductRepository
    {
        private readonly OrderContext _orderContext;

        public ProductRepository(OrderContext orderContext)
        {
            _orderContext = orderContext;
        }

        public async Task<bool> ExistsAsync(Guid productId,  Guid serviceId)
        {
            var productIdBytes = productId.ToByteArray();

            var serviceIdBytes = serviceId.ToByteArray();

            return await _orderContext.OrderProduct.AnyAsync(x => x.Id == productIdBytes && x.ServiceId == serviceIdBytes);
        }
    }
}
