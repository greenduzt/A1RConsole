using A1RConsole.Models.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.Models.Dispatch
{
    public class DispatchOrderItem : SalesOrderDetails
    {
        public Int32 DispatchOrderItemID { get; set; }
        public Int32 SalesNo { get; set; }
        //public Product Product { get; set; }
        public decimal OrderQty { get; set; }
        public decimal PackedQty { get; set; }
        public decimal DispatchQty { get; set; }
    }
}
