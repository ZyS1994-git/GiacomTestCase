using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Data
{
    public interface IServiceRepository
    {
        Task<bool> ExistsAsync(Guid serviceId);
    }
}
