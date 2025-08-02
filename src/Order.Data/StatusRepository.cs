using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Order.Data
{
    public class StatusRepository : IStatusRepository
    {
        private readonly OrderContext _orderContext;

        public StatusRepository(OrderContext orderContext)
        {
            _orderContext = orderContext;
        }
        public async Task<bool> ExistsAsync(string name)
        {
            return await _orderContext.OrderStatus.AnyAsync(status => status.Name.ToLower() == name.ToLower());
        }

        public async Task<bool> ExistsAsync(Guid statusId)
        {
            return await _orderContext.OrderStatus.AnyAsync(status => status.Id== statusId.ToByteArray());
        }
    }
}
