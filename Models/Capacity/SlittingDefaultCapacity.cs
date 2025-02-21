using A1RConsole.Models.Machines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.Models.Capacity
{
    public class SlittingDefaultCapacity
    {
        public int ID { get; set; }
        public Machine Machine { get; set; }
        public decimal DollarValue { get; set; }
        public int Shift { get; set; }
        public string Day { get; set; }
    }
}
