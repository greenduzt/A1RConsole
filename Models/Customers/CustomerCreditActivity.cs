using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.Models.Customers
{
    public class CustomerCreditActivity
    {
        public Customer Customer { get; set; }
        public Int32 SalesOrderNo { get; set; }
        public decimal Amount { get; set; }
        public string Type { get; set; }
        public string Activity { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string UpdatedBy { get; set; }
    }
}
