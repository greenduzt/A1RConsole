using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.Models.Products
{
    public class ProductMeterage
    {
        public int ID { get; set; }
        public decimal Thickness { get; set; }
        public decimal MouldSize { get; set; }
        public string MouldType { get; set; }
        public decimal ExpectedYield { get; set; }
    }
}
