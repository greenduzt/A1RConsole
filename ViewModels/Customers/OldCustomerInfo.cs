using A1RConsole.Models.Customers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.ViewModels.Customers
{
    public class OldCustomerInfo
    {
        private Customer _oldCustomer;
        public Customer OldCustomer
        {
            get
            {
                return _oldCustomer;
            }
            set
            {
                _oldCustomer = value;
            }
        }
    }
}
