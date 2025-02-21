using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.Models.Products
{
    public class ProductTransactions
    {
        public Int32 ID { get; set; }
        public DateTime TransactionDate { get; set; }
        public string Qty { get; set; }
        public string Type { get; set; }
        public string Reason { get; set; }
        public Int32 SalesNo { get; set; }
        public Int32 ShippingNo { get; set; }
        public Int32 PurchaseNo { get; set; }
        public Int32 WorkOrderNo { get; set; }
        public Product Product { get; set; }
        public int LineNo { get; set; }
        public string AddedBy { get; set; }
    }
}
