using System;
using System.Collections.Generic;

namespace Order.Model.Dtos
{
    public class PostOrderDto
    {
        public Guid ResellerId { get; set; }
        public Guid CustomerId { get; set; }
        public Guid  StatusId { get; set; }
        public DateTime CreatedDate { get; set; }
        public virtual List<OrderItemDto> Items { get; set; }
    }
}
