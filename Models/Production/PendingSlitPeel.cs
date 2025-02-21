using A1RConsole.Models.Orders;
using A1RConsole.Models.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.Models.Production
{
    public class PendingSlitPeel
    {
        public Int32 ID { get; set; }
        public Order Order { get; set; }
        public Product Product { get; set; }
        public decimal Qty { get; set; }
        public decimal BlockLogQty { get; set; }
        public bool Active { get; set; }
    }
}
