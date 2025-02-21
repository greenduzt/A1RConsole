using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.Models.Products
{
    public class Tile : Product
    {
        public int ID { get; set; }
        public decimal Height { get; set; }
        public decimal TilePerBlock { get; set; }
        public decimal Thickness { get; set; }
        public decimal MaxYield { get; set; }
        public decimal MinYield { get; set; }
    }
}
