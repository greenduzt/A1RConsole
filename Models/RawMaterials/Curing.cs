using A1RConsole.Models.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.Models.RawMaterials
{
    public class Curing
    {
        public int id { get; set; }
        public Int32 OrderNo { get; set; }
        public Product Product { get; set; }
        //public RawProduct RawProduct { get; set; }
        public decimal Qty { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsCured { get; set; }
        public bool IsEnabled { get; set; }
    }
}
