using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.Models.Customers
{
    public class CustomerCreditHistory
    {
        public Int32 ID { get; set; }
        public Customer Customer { get; set; }
        public Int32 SalesOrderNo { get; set; }
        public decimal CreditLimit { get; set; }
        public decimal CreditDeducted { get; set; }
        public decimal CreditRemaining { get; set; }
        public decimal Debt { get; set; }
        public decimal CreditAdded { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public decimal TotalCreditOwed { get; set; }
        public decimal TotalDebt { get; set; }
        public decimal TotalCreditRemaining { get; set; }
        public string Activity { get; set; }
        public bool Active { get; set; }
    }
}
