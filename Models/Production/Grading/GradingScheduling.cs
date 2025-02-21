using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.Models.Production.Grading
{
    public class GradingScheduling
    {
        public int ID { get; set; }
        public Int32 ProductionTimeTableID { get; set; }
        public int RawProductID { get; set; }
        public Int32 SalesID { get; set; }
        public decimal BlocklogQty { get; set; }
        public int Shift { get; set; }
        public string Status { get; set; }
        public int OrderType { get; set; }
        public bool ActiveOrder { get; set; }
        public int PrintCounter { get; set; }
        public bool RequiredDateSelected { get; set; }
        public DateTime RequiredDate { get; set; }
    }
}
