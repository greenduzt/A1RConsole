using A1RConsole.Models.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.Models.Stock
{
    public class ProductStockReserved
    {
        public Int32 ProductStockReservedID { get; set; }
        public Int32 SalesNo { get; set; }
        public StockLocation StockLocation { get; set; }
        public Product Product { get; set; }
        public decimal QtyOrdered { get; set; }
        public decimal QtyReserved { get; set; }
        public decimal QtyRemaining { get; set; }
        public DateTime ReservedDate { get; set; }
        public string Status { get; set; }
        public DateTime ActivityDate { get; set; }
        public int OrderLine { get; set; }
    }
}
