using A1RConsole.Models.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.Models.Production.Grading
{
    public class GradingCompleted
    {
        public Int32 ID { get; set; }
        public Int32 SalesID { get; set; }
        public Product RawProduct { get; set; }
        public Int32 ProdTimeTableID { get; set; }
        public int GradingID { get; set; }
        public string GradingName { get; set; }
        public decimal KGCompleted { get; set; }
        public int Shift { get; set; }
        public DateTime CreatedDate { get; set; }
        public int OrderType { get; set; }
        public decimal Qty { get; set; }
    }
}
