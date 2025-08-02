using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Service.Exceptions
{
    public class BadRequestException : Exception
    {
        public int StatusCode;
        public BadRequestException(string message) : base(message)
        {
            StatusCode = 400;
        }
    }
}
