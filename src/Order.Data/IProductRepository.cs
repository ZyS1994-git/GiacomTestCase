using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Data
{
    public interface IProductRepository
    {
        Task<bool> ExistsAsync(Guid productId, Guid serviceId);
    }
}
