using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.Models.Customers
{
    public class CustomerNote
    {
        public int ID { get; set; }
        public Int32 CustomerID { get; set; }
        public string Comment { get; set; }
        public string CreatedBy { get; set; }
        public DateTime DateTime { get; set; }
        public string DisplayString { get; set; }
    }
}
