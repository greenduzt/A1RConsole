using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.Models.Stock
{
    public class RawStock
    {
        public int ID { get; set; }
        public int RawProductID { get; set; }
        public decimal Qty { get; set; }
        public decimal ReOrderQty { get; set; }
        public decimal ReservedQty { get; set; }
        public DateTime LastUpdatedDate { get; set; }
    }
}
