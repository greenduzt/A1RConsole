using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.Models.Products
{
    public class RawProductsActive
    {
        public int ID { get; set; }
        public string Type { get; set; }
        public int RawProductID { get; set; }
        public bool DayShift { get; set; }
        public bool EveningShift { get; set; }
        public bool NightShift { get; set; }
    }
}
