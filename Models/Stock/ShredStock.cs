using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.Models.Stock
{
    public class ShredStock
    {
        public int ID { get; set; }
        public Shred Shred { get; set; }
        public decimal Qty { get; set; }
    }
}
