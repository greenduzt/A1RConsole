using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.Models.Products
{
    public class RawProductMachine
    {
        public int ID { get; set; }
        public int RawProductID { get; set; }
        public int GradingMachineID { get; set; }
        public int MixingMachineID { get; set; }
        public int SlitPeelMachineID { get; set; }
        public int ReRollingMachineID { get; set; }
    }
}
