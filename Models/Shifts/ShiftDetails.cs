using A1RConsole.Models.Stock;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.Models.Shifts
{
    public class ShiftDetails
    {
        public int ProTimeTableID { get; set; }
        public int GradingID { get; set; }
        public int shift { get; set; }
        public DateTime ProdDate { get; set; }
        public decimal CurrentCapacity { get; set; }
        public decimal TotCapacityKg { get; set; }
        public decimal GradedKg { get; set; }
        public List<GradedStock> GradedStock { get; set; }
    }
}
