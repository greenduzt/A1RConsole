using A1RConsole.Models.Dispatch;
using A1RConsole.Models.Products;
using A1RConsole.Models.Stock;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.Models.Orders
{
    public class SalesOrderLines
    {
        public SalesOrder SalesOrder { get; set; }
        public SupplierProduct SupplierProduct { get; set; }
        public DispatchOrderItem DispatchOrderItem { get; set; }
        public ProductStockReserved ProductStockReserved { get; set; }
    }
}
