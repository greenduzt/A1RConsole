using A1RConsole.Models.Stock;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.Models.Transactions
{
    public class TransactionLog
    {
        public DateTime TransDateTime { get; set; }
        public string Transtype { get; set; }
        public Int32 SalesOrderID { get; set; }
        public List<RawStock> Products { get; set; }
        //public int RawProductID { get; set; }
        //public decimal Qty { get; set; }
        public string CreatedBy { get; set; }
    }
}
