using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.Models.Stock
{
    public class GradedStock
    {
        public Int64 ID { get; set; }
        public string RowID { get; set; }
        public string GradeName { get; set; }
        public decimal Qty { get; set; }
        public decimal ReOrderQty { get; set; }

    }
}
