using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.Models.Vjs
{
    public class VjsNotInvoiced
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime DesiredSipDate { get; set; }
        public string OrderStatus { get; set; }
        public int LineNo { get; set; }
        public string PartID { get; set; }
        public string Description { get; set; }
        public string Unit { get; set; }
        public decimal OrderQty { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
