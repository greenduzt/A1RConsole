using A1RConsole.Models.Customers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.Models.Orders.OnlineOrders
{
    public class OnlineOrderCustomer : Customer
    {
        public string UserNiceName { get; set; }
        public string UserEmail { get; set; }
        public string DisplayName { get; set; }
    }
}
