using A1RConsole.Models.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.Models.Stock
{
    public class InventoryValue : Product
    {
        public StockLocation StockLocation { get; set; }
        public decimal TotalValue { get; set; }
        public decimal QtyOnHand { get; set; }
    }
}
