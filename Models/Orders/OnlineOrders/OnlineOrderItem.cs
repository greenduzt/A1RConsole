using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.Models.Orders.OnlineOrders
{
    public class OnlineOrderItem
    {
        public Int16 ID { get; set; }
        public Int16 OrderID { get; set; }
        public int BrandID { get; set; }
        public string BrandName { get; set; }
        public int ProductID { get; set; }
        public string ProductDetails { get; set; }
        public string ProductDescription { get; set; }
        public string ProductCode { get; set; }
        public string ProductUnit { get; set; }
        public int UnitsPerPackage { get; set; }
        public double UnitPrice { get; set; }
        public double PackPrice { get; set; }
        public int ProductQty { get; set; }
        public int Discount { get; set; }
        public double Cost { get; set; }        
    }
}
