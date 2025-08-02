using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Data
{
    public interface IStatusRepository
    {
        Task<bool> ExistsAsync(string name);
        Task<bool> ExistsAsync(Guid statusId);

    }
}
