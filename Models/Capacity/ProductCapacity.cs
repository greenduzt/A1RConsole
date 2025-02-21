using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.Models.Capacity
{
    public class ProductCapacity
    {
        public int ID { get; set; }
        public int ProductionTimeTableID { get; set; }
        public int RubberGradingID { get; set; }
        public int Shift { get; set; }
        public decimal CapacityKG { get; set; }
        public decimal GradedKG { get; set; }
    }
}
