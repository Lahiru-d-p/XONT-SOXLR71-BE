using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XONT.Common.Message;

namespace XONT.VENTURA.SOXLR71
{
    public class Result<T>
    {
        public T Data { get; set; }
        public MessageSet Message { get; set; }
        public bool IsSuccess => Message == null;
    }
}
