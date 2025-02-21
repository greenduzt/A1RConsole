using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.Models.Customers
{
    public class CustomerPending : Customer
    {
        public string DiscountStr { get; set; }
        public string ProductTypeStr { get; set; }
        public string CustomerNoteStr { get; set; }
        public bool IsTransfered { get; set; }
        public DateTime TransderedDateTime { get; set; }
        public string TransferedBy { get; set; }
    }
}
