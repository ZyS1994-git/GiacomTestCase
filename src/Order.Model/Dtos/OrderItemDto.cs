using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Model.Dtos
{
    public class OrderItemDto
    {
        public Guid ProductId { get; set; }
        public Guid ServiceId { get; set; }
        public int Quantity { get; set; } = 1;
    }
}
