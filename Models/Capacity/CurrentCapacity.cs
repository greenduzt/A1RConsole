using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.Models.Capacity
{
    public class CurrentCapacity
    {
        public int ID { get; set; }
        public int ProdTimeTableID { get; set; }
        public DateTime GradingDate { get; set; }
        public int ProductCapacityID { get; set; }
        public int SalesID { get; set; }
        public int RawProductID { get; set; }
        public int ProductID { get; set; }
        public int Shift { get; set; }
        public decimal CapacityKG { get; set; }
        public decimal BlocksLogs { get; set; }
        public bool Paired { get; set; }
        public int OrderType { get; set; }
    }
}
